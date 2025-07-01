# LLM Arena RAG System Update

## Overview

The LLM Arena system has been updated to use real LLM calls with RAG (Retrieval-Augmented Generation) instead of stub answers. The system now:

1. **Builds proper vector embeddings** for document chunks using each provider's embedding API
2. **Retrieves relevant context** from the vector store when answering questions
3. **Generates real answers** using LLMs with the retrieved context

## Key Changes

### 1. Adapter Updates

All LLM adapters now implement:
- **`embed()` method**: Generates embeddings using the provider's embedding API
- **`generate()` method**: Generates answers with RAG context

#### OpenAI Adapter
- Uses `text-embedding-3-large` for embeddings
- Supports RAG context in generation

#### Claude Adapter  
- Uses OpenAI embeddings (since Anthropic doesn't have embedding models)
- Requires separate `embedding_api_key` in config
- Supports RAG context in generation

#### Gemini Adapter
- Uses `models/text-embedding-004` for embeddings
- Supports RAG context in generation

### 2. RAG Builder Updates

- **Real embeddings**: No more random vectors - uses actual embedding APIs
- **Progress tracking**: Shows embedding progress for each chunk
- **Error handling**: Skips failed embeddings and continues processing

### 3. Answer Generation Updates

- **Real LLM calls**: Generates actual answers instead of stub responses
- **RAG integration**: Retrieves relevant context from vector store
- **Error handling**: Graceful fallback for failed generations

## Configuration

### Updated config.yaml

```yaml
llm_providers:
  - name: gemini-2.5-pro
    api_key: YOUR_GEMINI_API_KEY
    embedding_model: models/text-embedding-004
    generation_model: gemini-2.5-pro
    
  - name: openai-gpt-4o-mini
    api_key: YOUR_OPENAI_API_KEY
    embedding_model: text-embedding-3-large
    generation_model: gpt-4o-mini
    
  - name: claude-3-5-sonnet
    api_key: YOUR_ANTHROPIC_API_KEY
    embedding_api_key: YOUR_OPENAI_API_KEY  # For embeddings
    embedding_model: text-embedding-3-large
    generation_model: claude-3-5-sonnet-20241022
```

## Usage

### 1. Build RAG Systems

```bash
cd "LLM Arena"
python main.py build
```

This will:
- Load documents from `./data/source_docs`
- Chunk them according to `chunk_size` and `chunk_overlap`
- Generate embeddings for each chunk using each provider's API
- Build FAISS indices and save them to `./vector_stores/{provider_name}/`

### 2. Generate Questions

```bash
python main.py generate-questions
```

### 3. Generate Answers (Now with Real LLM Calls)

```bash
python main.py generate-answers-cmd
```

This will:
- Load questions from `./output/curated_questions.json`
- For each question and provider:
  - Embed the question using the provider's embedding API
  - Retrieve top 3 most similar chunks from the vector store
  - Generate an answer using the LLM with the retrieved context
- Save results to `./output/generated_answers.json`

### 4. Test the System

```bash
python test_rag.py
```

This runs a quick test with a single question to verify the RAG system is working.

## File Structure

```
LLM Arena/
├── adapters/
│   ├── base.py              # Base adapter class
│   ├── openai_adapter.py    # OpenAI adapter with embeddings
│   ├── claude_adapter.py    # Claude adapter with OpenAI embeddings
│   └── gemini_adapter.py    # Gemini adapter with embeddings
├── vector_stores/
│   ├── openai-gpt-4o-mini/
│   │   ├── index.faiss      # FAISS index
│   │   └── documents.pkl    # Document chunks mapping
│   ├── claude-3-5-sonnet/
│   └── gemini-2.5-pro/
├── output/
│   ├── curated_questions.json
│   └── generated_answers.json
├── rag_builder.py           # Updated to use real embeddings
├── answer_gen.py            # Updated to use real LLM calls
├── test_rag.py              # Test script
└── config.yaml              # Updated configuration
```

## Error Handling

The system includes comprehensive error handling:

- **Embedding failures**: Logs errors and continues with other chunks
- **Generation failures**: Uses fallback error messages
- **Missing files**: Warns and skips providers with missing vector stores
- **API errors**: Logs detailed error messages for debugging

## Performance Notes

- **Embedding generation**: Can take time for large document sets
- **API costs**: Real LLM calls will incur API costs
- **Rate limiting**: Built-in error handling for API rate limits
- **Caching**: Vector stores are cached locally for reuse

## Troubleshooting

### Common Issues

1. **Missing API keys**: Ensure all API keys are valid and have sufficient credits
2. **Rate limiting**: The system handles rate limits gracefully, but may take longer
3. **Vector store missing**: Run `python main.py build` first
4. **Embedding dimension mismatch**: Each provider uses different embedding dimensions

### Debug Mode

Add debug logging by modifying the config or adding print statements in the adapters.

## Next Steps

The system is now ready for:
- Human evaluation using the UI
- Comparative analysis of different LLM providers
- Performance benchmarking
- Further customization of prompts and retrieval strategies 