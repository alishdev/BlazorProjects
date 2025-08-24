from database import DatabaseManager
from embeddings import EmbeddingsManager
from document_processor import DocumentProcessor
from config import Config
import logging
import json
from typing import List, Dict, Any

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class RAGSystem:
    def __init__(self):
        """Initialize the RAG system with database, embeddings, and document processing"""
        try:
            self.db_manager = DatabaseManager()
            self.embeddings_manager = EmbeddingsManager()
            self.doc_processor = DocumentProcessor()
            logger.info("RAG system initialized successfully")
        except Exception as e:
            logger.error(f"Failed to initialize RAG system: {e}")
            raise
    
    def upload_documents(self, pdf_directory: str) -> Dict[str, Any]:
        """Upload and process all PDF documents from a directory"""
        try:
            logger.info(f"Starting document upload from directory: {pdf_directory}")
            
            # Process all PDF documents
            processed_chunks = self.doc_processor.process_pdf_directory(pdf_directory)
            
            if not processed_chunks:
                logger.warning("No PDF documents found in the directory")
                return {"status": "warning", "message": "No PDF documents found", "chunks_processed": 0}
            
            # Generate embeddings for all chunks
            chunk_texts = [chunk['content'] for chunk in processed_chunks]
            logger.info(f"Generating embeddings for {len(chunk_texts)} chunks...")
            
            embeddings = self.embeddings_manager.generate_embeddings_batch(chunk_texts)
            
            # Store documents and embeddings in database
            chunks_stored = 0
            chunks_skipped = 0
            
            for i, (chunk, embedding) in enumerate(zip(processed_chunks, embeddings)):
                try:
                    # Store document chunk
                    document_id = self.db_manager.insert_document_chunk(
                        filename=chunk['filename'],
                        content=chunk['content'],
                        chunk_index=chunk['chunk_index'],
                        metadata=json.dumps(chunk)
                    )
                    
                    # Skip if document insertion failed (e.g., empty content)
                    if document_id is None:
                        chunks_skipped += 1
                        logger.warning(f"Skipped chunk {i} from {chunk['filename']} (empty or invalid content)")
                        continue
                    
                    # Store embedding (will be automatically truncated to 2000 dimensions)
                    self.embeddings_manager.validate_embedding_dimension(embedding)
                    self.db_manager.insert_embedding(document_id, embedding)
                    
                    chunks_stored += 1
                    
                    if (i + 1) % 10 == 0:
                        logger.info(f"Processed {i + 1}/{len(processed_chunks)} chunks")
                        
                except Exception as e:
                    logger.error(f"Error storing chunk {i}: {e}")
                    chunks_skipped += 1
                    continue
            
            logger.info(f"Document upload completed. {chunks_stored}/{len(processed_chunks)} chunks stored successfully, {chunks_skipped} skipped")
            
            return {
                "status": "success",
                "chunks_processed": len(processed_chunks),
                "chunks_stored": chunks_stored,
                "chunks_skipped": chunks_skipped,
                "files_processed": len(set(chunk['filename'] for chunk in processed_chunks))
            }
            
        except Exception as e:
            logger.error(f"Error during document upload: {e}")
            return {"status": "error", "message": str(e)}
    
    def search_documents(self, query: str, top_k: int = None) -> List[Dict[str, Any]]:
        """Search for relevant documents based on a query"""
        try:
            if top_k is None:
                top_k = Config.TOP_K_RESULTS
            
            logger.info(f"Searching for query: '{query}' with top_k={top_k}")
            
            # Generate embedding for the query
            query_embedding = self.embeddings_manager.generate_embedding(query)
            
            # Search for similar documents (query embedding will be automatically adjusted to 2000 dimensions)
            search_results = self.db_manager.search_similar_documents(query_embedding, top_k)
            
            # Format results
            formatted_results = []
            for result in search_results:
                formatted_result = {
                    'id': result['id'],
                    'filename': result['filename'],
                    'content': result['content'],
                    'chunk_index': result['chunk_index'],
                    'metadata': result['metadata'],
                    'similarity_score': float(result['similarity'])
                }
                formatted_results.append(formatted_result)
            
            logger.info(f"Search completed. Found {len(formatted_results)} relevant documents")
            return formatted_results
            
        except Exception as e:
            logger.error(f"Error during document search: {e}")
            return []
    
    def get_system_stats(self) -> Dict[str, Any]:
        """Get system statistics"""
        try:
            document_count = self.db_manager.get_document_count()
            
            return {
                "total_documents": document_count,
                "original_vector_dimension": Config.VECTOR_DIMENSION,
                "database_vector_dimension": Config.DB_VECTOR_DIMENSION,
                "chunk_size": Config.CHUNK_SIZE,
                "chunk_overlap": Config.CHUNK_OVERLAP,
                "embedding_model": Config.JINA_MODEL_NAME
            }
            
        except Exception as e:
            logger.error(f"Error getting system stats: {e}")
            return {"error": str(e)}
    
    def close(self):
        """Clean up resources"""
        try:
            self.db_manager.close()
            logger.info("RAG system resources cleaned up")
        except Exception as e:
            logger.error(f"Error during cleanup: {e}")
