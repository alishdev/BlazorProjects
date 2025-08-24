# RAG System with PostgreSQL, pgvector, and Jina AI

A comprehensive Retrieval-Augmented Generation (RAG) system that uses PostgreSQL with pgvector extension for vector storage and [Jina AI's embeddings API](https://docs.jina.ai/) for generating high-quality embeddings.

## Features

- **PDF Document Processing**: Automatically extracts and chunks text from PDF documents
- **Vector Embeddings**: Uses Jina AI's state-of-the-art [jina-embeddings-v4](https://docs.jina.ai/) model (3.8B parameters, 2048 dimensions)
- **Vector Database**: PostgreSQL with pgvector extension for efficient similarity search
- **Smart Chunking**: Intelligent text chunking with configurable overlap
- **Batch Processing**: Efficient batch embedding generation
- **Interactive Search**: Command-line interface for querying the knowledge base
- **Comprehensive Logging**: Detailed logging for monitoring and debugging

## Prerequisites

### 1. PostgreSQL with pgvector Extension

First, install PostgreSQL and the pgvector extension:

```bash
# On macOS with Homebrew
brew install postgresql
brew install pgvector

# On Ubuntu/Debian
sudo apt-get update
sudo apt-get install postgresql postgresql-contrib
sudo apt-get install postgresql-14-pgvector  # Adjust version as needed

# On CentOS/RHEL
sudo yum install postgresql postgresql-contrib
sudo yum install pgvector_14  # Adjust version as needed
```

### 2. Python Dependencies

Install the required Python packages:

```bash
python3 -m venv venv
   source venv/bin/activate
pip install -r requirements.txt
```

### 3. Environment Setup

Create a `.env` file in the project root:

```bash
cp env_example.txt .env
```

Edit the `.env` file with your configuration:

```env
# Database Configuration
DATABASE_URL=postgresql://username:password@localhost:5432/rag_db

# Jina AI Configuration
JINA_API_KEY=your_jina_api_key_here
```

## Setup Instructions

### 1. Get Jina AI API Key

1. Visit [Jina AI](https://jina.ai/)
2. Sign up for an account
3. Navigate to the API keys section
4. Generate a new API key
5. Add it to your `.env` file

**Get your Jina AI API key for free: [https://jina.ai/?sui=apikey](https://jina.ai/?sui=apikey)**

### 2. Database Setup

Create a PostgreSQL database and enable the pgvector extension:

```sql
-- Connect to PostgreSQL as superuser
sudo -u postgres psql

-- Create database
CREATE DATABASE rag_db;

-- Connect to the new database
\c rag_db

-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create a user (optional but recommended)
CREATE USER rag_user WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE rag_db TO rag_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO rag_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO rag_user;

-- Exit
\q
```

## Usage

### Basic Usage

1. **Prepare your PDF documents** in a directory
2. **Run the RAG system**:

```bash
python main.py /path/to/your/pdf/documents
```

3. **Follow the interactive prompts** to search your documents

### Example Session

```bash
source venv/bin/activate
python main.py ./documents

✅ RAG system initialized successfully

📊 System Statistics (before upload):
  total_documents: 0
  vector_dimension: 2048
  chunk_size: 1000
  chunk_overlap: 200
  embedding_model: jina-embeddings-v4

📚 Uploading documents from: ./documents
✅ Document upload completed successfully!
  Chunks processed: 45
  Chunks stored: 45
  Files processed: 3

📊 System Statistics (after upload):
  total_documents: 45
  vector_dimension: 2048
  chunk_size: 1000
  chunk_overlap: 200
  embedding_model: jina-embeddings-v4

🔍 Interactive Search Mode
Enter your search queries (type 'quit' to exit):

Enter search query: machine learning applications
📄 Found 3 relevant documents:

--- Result 1 ---
📁 File: research_paper.pdf
🔢 Chunk: 12
📊 Similarity: 0.8923
📝 Content: Machine learning applications in healthcare have shown promising results...

Enter search query: quit

👋 Goodbye!
✅ RAG system shutdown complete
```

### Programmatic Usage

You can also use the RAG system programmatically:

```python
from rag_system import RAGSystem

# Initialize the system
rag = RAGSystem()

# Upload documents
result = rag.upload_documents("./documents")
print(f"Uploaded {result['chunks_stored']} chunks")

# Search for documents
results = rag.search_documents("your search query", top_k=5)
for result in results:
    print(f"File: {result['filename']}, Similarity: {result['similarity_score']}")

# Clean up
rag.close()
```

## Configuration

You can customize the system behavior by modifying `config.py` or setting environment variables:

- **CHUNK_SIZE**: Size of text chunks (default: 1000 characters)
- **CHUNK_OVERLAP**: Overlap between chunks (default: 200 characters)
- **VECTOR_DIMENSION**: Embedding vector dimension (default: 2048 for jina-embeddings-v4)
- **TOP_K_RESULTS**: Number of results to return (default: 5)

## Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   PDF Documents │───▶│ DocumentProcessor│───▶│  Text Chunks   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                                         │
                                                         ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  Search Query  │───▶│ EmbeddingsManager│◀───│  Jina AI API   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                                         │
                                                         ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│ Search Results  │◀───│ DatabaseManager  │◀───│  Vector Search  │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                                         │
                                                         ▼
                                               ┌─────────────────┐
                                               │ PostgreSQL +    │
                                               │   pgvector      │
                                               └─────────────────┘
```

## Performance Considerations

- **Batch Processing**: The system processes embeddings in batches to optimize API usage
- **Vector Indexing**: Uses IVFFlat indexing for efficient similarity search
- **Connection Pooling**: Database connections are managed efficiently
- **Memory Management**: Large documents are processed in chunks to manage memory usage

## Troubleshooting

### Common Issues

1. **Database Connection Error**
   - Verify PostgreSQL is running
   - Check your DATABASE_URL in the .env file
   - Ensure pgvector extension is installed

2. **Jina AI API Error**
   - Verify your JINA_API_KEY is correct
   - Check your internet connection
   - Ensure you have sufficient API credits
   - Get your free API key: [https://jina.ai/?sui=apikey](https://jina.ai/?sui=apikey)

3. **PDF Processing Error**
   - Ensure PDF files are not corrupted
   - Check file permissions
   - Verify PyPDF2 is properly installed

### Debug Mode

Enable debug logging by modifying the logging level in any module:

```python
logging.basicConfig(level=logging.DEBUG)
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and questions:
1. Check the troubleshooting section
2. Review the logs for error messages
3. Open an issue on GitHub with detailed information about your setup and error
4. Get help with Jina AI: [https://docs.jina.ai/](https://docs.jina.ai/)
