# LLM Arena - ChromaDB-based RAG Evaluation System

This is a ChromaDB-based version of the LLM Arena evaluation system. It provides the same functionality as the original FAISS-based system but uses ChromaDB as the vector database backend for RAG (Retrieval-Augmented Generation).

## Key Differences from FAISS Version

- **Vector Database**: Uses ChromaDB instead of FAISS for vector storage and similarity search
- **Persistence**: ChromaDB provides better persistence and metadata storage capabilities
- **Metadata Support**: Enhanced metadata handling for document chunks
- **Scalability**: Better suited for larger document collections and production deployments

## Features

- **Multi-LLM Support**: Evaluate OpenAI GPT-4, Claude, and Gemini models
- **RAG Pipeline**: Build vector stores, generate questions, and create answers with context
- **Human Evaluation UI**: Streamlit and Gradio interfaces for human evaluation
- **Configurable**: YAML-based configuration for easy customization

## Installation

1. Create a virtual environment:
```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

3. Configure your API keys in `config.yaml`

## Usage

### Initialize the System
```bash
python main.py init
```

### Build RAG Systems
```bash
python main.py build
```

### Generate Questions
```bash
python main.py questions
```

### Generate Answers
```bash
python main.py answers
```

### Run Complete Pipeline
```bash
python main.py run-all
```

### Launch Evaluation UI

**Streamlit:**
```bash
streamlit run eval_ui.py
```

**Gradio:**
```bash
python eval_ui_gradio.py
```

## Configuration

Edit `config.yaml` to configure:
- LLM provider API keys and models
- Document paths
- Chunking parameters
- Prompts for question and answer generation

## Directory Structure

```
LLM Arena Chroma/
├── adapters/           # LLM provider adapters
├── data/              # Source documents
├── vector_stores/     # ChromaDB vector stores
├── output/            # Generated questions and answers
├── config.yaml        # Configuration file
├── main.py           # Main CLI interface
├── rag_builder.py    # ChromaDB RAG system builder
├── question_gen.py   # Question generation
├── answer_gen.py     # Answer generation
├── eval_ui.py        # Streamlit evaluation UI
├── eval_ui_gradio.py # Gradio evaluation UI
└── requirements.txt  # Python dependencies
```

## ChromaDB Collections

Each LLM provider gets its own ChromaDB collection:
- `gemini_2_5_pro_collection`
- `openai_gpt_4o_mini_collection`
- `claude_3_5_sonnet_collection`

Collections are stored in `vector_stores/{provider_name}/` directories.

## Advantages of ChromaDB over FAISS

1. **Metadata Storage**: ChromaDB stores document metadata alongside embeddings
2. **Persistence**: Better handling of large datasets and persistence
3. **Query Flexibility**: More flexible querying capabilities
4. **Production Ready**: Better suited for production deployments
5. **Versioning**: Built-in support for collection versioning

## Troubleshooting

- Ensure all API keys are correctly configured in `config.yaml`
- Check that source documents exist in the specified path
- Verify ChromaDB collections are created before running question/answer generation
- Monitor API rate limits for your LLM providers 