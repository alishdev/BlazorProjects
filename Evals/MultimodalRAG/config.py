import os
from dotenv import load_dotenv

load_dotenv()

class Config:
    # Database configuration
    DATABASE_URL = os.getenv("DATABASE_URL", "postgresql://localhost:5432/rag_db")
    
    # Jina AI configuration
    JINA_API_KEY = os.getenv("JINA_API_KEY")
    JINA_MODEL_NAME = "jina-embeddings-v4"  # 3.8B model with 2048 dimensions
    
    # Vector dimensions for jina-embeddings-v4 (truncated to 2000 for PostgreSQL)
    VECTOR_DIMENSION = 2048  # Original embedding dimension from jina-embeddings-v4
    DB_VECTOR_DIMENSION = 2000  # Truncated dimension for PostgreSQL storage
    
    # Chunking configuration
    CHUNK_SIZE = 1000
    CHUNK_OVERLAP = 200
    
    # Search configuration
    TOP_K_RESULTS = 5
