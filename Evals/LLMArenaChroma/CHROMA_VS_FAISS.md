# ChromaDB vs FAISS: LLM Arena Comparison

This document outlines the key differences between the FAISS-based and ChromaDB-based versions of the LLM Arena evaluation system.

## Architecture Differences

### FAISS Version
- **Vector Storage**: Uses FAISS (Facebook AI Similarity Search) for vector indexing
- **Document Storage**: Separate pickle files for document metadata
- **File Structure**: 
  ```
  vector_stores/
  ├── provider_name/
  │   ├── index.faiss          # FAISS index file
  │   └── documents.pkl        # Document metadata pickle file
  ```

### ChromaDB Version
- **Vector Storage**: Uses ChromaDB for vector storage and similarity search
- **Document Storage**: Integrated metadata storage within ChromaDB collections
- **File Structure**:
  ```
  vector_stores/
  ├── provider_name/
  │   ├── chroma.sqlite3       # ChromaDB database
  │   └── chroma/              # ChromaDB data directory
  ```

## Code Changes

### 1. Dependencies

**FAISS Version:**
```python
import faiss
import pickle
```

**ChromaDB Version:**
```python
import chromadb
from chromadb.config import Settings
```

### 2. RAG Builder (`rag_builder.py`)

**FAISS Version:**
```python
# Build and save FAISS index
index = faiss.IndexFlatL2(embedding_dim)
index.add(embeddings)
faiss.write_index(index, os.path.join(provider_dir, "index.faiss"))

# Save mapping file
with open(os.path.join(provider_dir, "documents.pkl"), "wb") as f:
    pickle.dump(all_chunks, f)
```

**ChromaDB Version:**
```python
# Initialize ChromaDB client
chroma_client = chromadb.PersistentClient(
    path=provider_dir,
    settings=Settings(anonymized_telemetry=False)
)

# Create or get collection
collection = chroma_client.create_collection(name=collection_name)

# Add documents to ChromaDB collection
collection.add(
    embeddings=embeddings,
    documents=documents,
    metadatas=metadatas,
    ids=ids
)
```

### 3. Answer Generation (`answer_gen.py`)

**FAISS Version:**
```python
# Load FAISS index and mapping
index = faiss.read_index(index_path)
with open(mapping_path, "rb") as f:
    all_chunks = pickle.load(f)

# Retrieve top chunks
D, I = index.search(vector.reshape(1, -1), 3)
retrieved = [all_chunks[i][0] for i in I[0] if i < len(all_chunks)]
```

**ChromaDB Version:**
```python
# Initialize ChromaDB client
chroma_client = chromadb.PersistentClient(path=provider_dir)
collection = chroma_client.get_collection(name=collection_name)

# Query ChromaDB for similar documents
query_results = collection.query(
    query_embeddings=[vector],
    n_results=3
)
retrieved_docs = query_results['documents'][0]
```

### 4. Question Generation (`question_gen.py`)

**FAISS Version:**
```python
# Load documents from pickle file
with open(mapping_path, "rb") as f:
    all_chunks = pickle.load(f)
```

**ChromaDB Version:**
```python
# Get all documents from ChromaDB collection
all_results = collection.get()
documents = all_results['documents']
```

## Performance Comparison

### FAISS Advantages
- **Speed**: Generally faster for large-scale similarity search
- **Memory Efficiency**: Lower memory footprint for large datasets
- **Mature**: Well-established library with extensive optimization

### ChromaDB Advantages
- **Metadata Integration**: Built-in metadata storage and querying
- **Persistence**: Better handling of data persistence and recovery
- **Flexibility**: More flexible querying capabilities
- **Production Ready**: Better suited for production deployments
- **Versioning**: Built-in support for collection versioning

## Use Case Recommendations

### Choose FAISS when:
- You need maximum performance for large-scale similarity search
- Memory usage is a critical constraint
- You're working with very large datasets (>1M vectors)
- You need fine-grained control over indexing algorithms

### Choose ChromaDB when:
- You need rich metadata storage and querying
- You're building a production system
- You need flexible querying capabilities
- You want better data persistence and recovery
- You're working with smaller to medium datasets (<1M vectors)

## Migration Guide

To migrate from FAISS to ChromaDB:

1. **Install ChromaDB**: `pip install chromadb`
2. **Update requirements.txt**: Replace `faiss-cpu` with `chromadb`
3. **Update imports**: Replace FAISS imports with ChromaDB imports
4. **Modify RAG builder**: Use ChromaDB collection operations instead of FAISS
5. **Update retrieval logic**: Use ChromaDB query methods instead of FAISS search
6. **Test thoroughly**: Verify that retrieval quality is maintained

## File Size Comparison

For the same dataset:
- **FAISS**: Typically smaller file sizes due to optimized binary format
- **ChromaDB**: Larger file sizes due to SQLite database and metadata storage

## Memory Usage

- **FAISS**: Lower memory usage, especially for large datasets
- **ChromaDB**: Higher memory usage due to metadata storage and SQLite overhead

## Conclusion

Both versions provide the same core functionality for LLM evaluation. The choice between FAISS and ChromaDB depends on your specific requirements:

- **Performance-critical applications**: Choose FAISS
- **Production systems with rich metadata**: Choose ChromaDB
- **Research and development**: ChromaDB provides more flexibility
- **Large-scale deployments**: FAISS may be more suitable

The ChromaDB version is recommended for most use cases due to its better metadata handling, persistence, and production readiness. 