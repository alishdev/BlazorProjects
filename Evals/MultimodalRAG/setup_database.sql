-- RAG System Database Setup Script
-- Run this script as a PostgreSQL superuser to set up the database

-- Create database (if it doesn't exist)
-- Note: You may need to create this manually or run: createdb rag_db

-- Connect to the rag_db database first, then run these commands:

-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create documents table
CREATE TABLE IF NOT EXISTS documents (
    id SERIAL PRIMARY KEY,
    filename VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    chunk_index INTEGER NOT NULL,
    metadata JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create embeddings table with vector column
-- Note: 2000 is the dimension for jina-embeddings-v4 (truncated from 2028 for PostgreSQL compatibility)
CREATE TABLE IF NOT EXISTS document_embeddings (
    id SERIAL PRIMARY KEY,
    document_id INTEGER REFERENCES documents(id) ON DELETE CASCADE,
    embedding vector(2000),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_document_embeddings_vector 
ON document_embeddings USING hnsw (embedding vector_cosine_ops)
WITH (m = 16, ef_construction = 64);

CREATE INDEX IF NOT EXISTS idx_documents_filename 
ON documents(filename);

CREATE INDEX IF NOT EXISTS idx_documents_created_at 
ON documents(created_at);

-- Create a user for the application (optional but recommended)
-- Replace 'your_password' with a secure password
CREATE USER rag_user WITH PASSWORD 'your_password';

-- Grant necessary privileges
GRANT CONNECT ON DATABASE rag_db TO rag_user;
GRANT USAGE ON SCHEMA public TO rag_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO rag_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO rag_user;

-- Grant privileges on future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO rag_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO rag_user;

-- Verify the setup
SELECT 
    'pgvector extension' as component,
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'vector') 
        THEN '✅ Installed' 
        ELSE '❌ Not installed' 
    END as status
UNION ALL
SELECT 
    'documents table' as component,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'documents') 
        THEN '✅ Created' 
        ELSE '❌ Not created' 
    END as status
UNION ALL
SELECT 
    'embeddings table' as component,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'document_embeddings') 
        THEN '✅ Created' 
        ELSE '❌ Not created' 
    END as status
UNION ALL
SELECT 
    'vector indexes' as component,
    CASE 
        WHEN EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_document_embeddings_vector') 
        THEN '✅ Created' 
        ELSE '❌ Not created' 
    END as status;

-- Show current database size
SELECT 
    pg_size_pretty(pg_database_size(current_database())) as database_size;
