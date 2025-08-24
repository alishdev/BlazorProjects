import psycopg2
from psycopg2.extras import RealDictCursor
import pgvector
from config import Config
import logging
import re
import numpy as np

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class DatabaseManager:
    def __init__(self):
        self.connection = None
        self.connect()
        self.setup_database()
    
    def connect(self):
        """Establish connection to PostgreSQL database"""
        try:
            self.connection = psycopg2.connect(Config.DATABASE_URL)
            # For pgvector 0.4.1+, we don't need to register_vector
            # The extension is automatically available when connected to a database with pgvector
            logger.info("Successfully connected to PostgreSQL database")
        except Exception as e:
            logger.error(f"Error connecting to database: {e}")
            raise
    
    def clean_text_for_db(self, text: str) -> str:
        """Clean text specifically for database insertion"""
        if not text:
            return ""
        
        # Remove NUL characters and other problematic characters
        text = text.replace('\x00', '')
        text = re.sub(r'[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]', '', text)
        
        # Normalize whitespace
        text = re.sub(r'\s+', ' ', text)
        text = text.strip()
        
        # Ensure the text is valid UTF-8
        text = text.encode('utf-8', errors='ignore').decode('utf-8')
        
        return text
    
    def setup_database(self):
        """Create necessary tables and extensions"""
        try:
            with self.connection.cursor() as cursor:
                # Check if pgvector extension is available
                cursor.execute("SELECT * FROM pg_extension WHERE extname = 'vector'")
                if not cursor.fetchone():
                    logger.warning("pgvector extension not found. Please ensure it's installed in your database.")
                
                # Create documents table
                cursor.execute("""
                    CREATE TABLE IF NOT EXISTS documents (
                        id SERIAL PRIMARY KEY,
                        filename VARCHAR(255) NOT NULL,
                        content TEXT NOT NULL,
                        chunk_index INTEGER NOT NULL,
                        metadata JSONB,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    )
                """)
                
                # Create embeddings table with vector column (2000 dimensions for PostgreSQL)
                cursor.execute("""
                    CREATE TABLE IF NOT EXISTS document_embeddings (
                        id SERIAL PRIMARY KEY,
                        document_id INTEGER REFERENCES documents(id) ON DELETE CASCADE,
                        embedding vector(%s),
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    )
                """, (Config.DB_VECTOR_DIMENSION,))
                
                # Create indexes for better performance
                # Use HNSW indexing for 2000 dimensions (within Supabase limits)
                cursor.execute("""
                    CREATE INDEX IF NOT EXISTS idx_document_embeddings_vector 
                    ON document_embeddings USING hnsw (embedding vector_cosine_ops)
                    WITH (m = 16, ef_construction = 64)
                """)
                
                cursor.execute("""
                    CREATE INDEX IF NOT EXISTS idx_documents_filename 
                    ON documents(filename)
                """)
                
                self.connection.commit()
                logger.info("Database setup completed successfully")
                
        except Exception as e:
            logger.error(f"Error setting up database: {e}")
            self.connection.rollback()
            raise
    
    def insert_document_chunk(self, filename, content, chunk_index, metadata=None):
        """Insert a document chunk into the database"""
        try:
            # Clean the content before insertion
            cleaned_content = self.clean_text_for_db(content)
            if not cleaned_content:
                logger.warning(f"Skipping empty chunk {chunk_index} from {filename}")
                return None
            
            # Clean the filename as well
            cleaned_filename = self.clean_text_for_db(filename)
            
            with self.connection.cursor() as cursor:
                cursor.execute("""
                    INSERT INTO documents (filename, content, chunk_index, metadata)
                    VALUES (%s, %s, %s, %s)
                    RETURNING id
                """, (cleaned_filename, cleaned_content, chunk_index, metadata))
                
                document_id = cursor.fetchone()[0]
                self.connection.commit()
                return document_id
                
        except Exception as e:
            logger.error(f"Error inserting document chunk: {e}")
            self.connection.rollback()
            raise
    
    def insert_embedding(self, document_id, embedding):
        """Insert an embedding vector into the database"""
        try:
            # Ensure the embedding has the correct dimension for the database
            if embedding.shape[0] != Config.DB_VECTOR_DIMENSION:
                logger.warning(f"Embedding dimension mismatch. Expected {Config.DB_VECTOR_DIMENSION}, got {embedding.shape[0]}")
                # Truncate if too long, pad if too short
                if embedding.shape[0] > Config.DB_VECTOR_DIMENSION:
                    embedding = embedding[:Config.DB_VECTOR_DIMENSION]
                else:
                    # Pad with zeros if too short
                    padding = np.zeros(Config.DB_VECTOR_DIMENSION - embedding.shape[0], dtype=np.float32)
                    embedding = np.concatenate([embedding, padding])
            
            # Convert to list and ensure proper vector type
            embedding_list = embedding.tolist()
            
            with self.connection.cursor() as cursor:
                cursor.execute("""
                    INSERT INTO document_embeddings (document_id, embedding)
                    VALUES (%s, %s::vector)
                """, (document_id, embedding_list))
                
                self.connection.commit()
                
        except Exception as e:
            logger.error(f"Error inserting embedding: {e}")
            self.connection.rollback()
            raise
    
    def search_similar_documents(self, query_embedding, top_k=5):
        """Search for similar documents using cosine similarity"""
        try:
            # Ensure query embedding has the correct dimension
            if query_embedding.shape[0] != Config.DB_VECTOR_DIMENSION:
                if query_embedding.shape[0] > Config.DB_VECTOR_DIMENSION:
                    query_embedding = query_embedding[:Config.DB_VECTOR_DIMENSION]
                else:
                    padding = np.zeros(Config.DB_VECTOR_DIMENSION - query_embedding.shape[0], dtype=np.float32)
                    query_embedding = np.concatenate([query_embedding, padding])
            
            # Convert to list and ensure proper vector type
            query_embedding_list = query_embedding.tolist()
            
            with self.connection.cursor(cursor_factory=RealDictCursor) as cursor:
                cursor.execute("""
                    SELECT 
                        d.id,
                        d.filename,
                        d.content,
                        d.chunk_index,
                        d.metadata,
                        1 - (de.embedding <=> %s::vector) as similarity
                    FROM document_embeddings de
                    JOIN documents d ON de.document_id = d.id
                    ORDER BY de.embedding <=> %s::vector
                    LIMIT %s
                """, (query_embedding_list, query_embedding_list, top_k))
                
                results = cursor.fetchall()
                return results
                
        except Exception as e:
            logger.error(f"Error searching documents: {e}")
            raise
    
    def get_document_count(self):
        """Get total number of documents in the database"""
        try:
            with self.connection.cursor() as cursor:
                cursor.execute("SELECT COUNT(*) FROM documents")
                return cursor.fetchone()[0]
        except Exception as e:
            logger.error(f"Error getting document count: {e}")
            return 0
    
    def close(self):
        """Close database connection"""
        if self.connection:
            self.connection.close()
            logger.info("Database connection closed")
