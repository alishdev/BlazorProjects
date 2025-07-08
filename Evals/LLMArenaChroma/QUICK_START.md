# Quick Start Guide - LLM Arena ChromaDB

This guide will help you get started with the ChromaDB-based LLM Arena evaluation system.

## Prerequisites

- Python 3.8 or higher
- API keys for OpenAI, Anthropic, and/or Google AI

## Installation

1. **Navigate to the ChromaDB directory:**
   ```bash
   cd "LLM Arena Chroma"
   ```

2. **Create a virtual environment:**
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```

3. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

4. **Configure your API keys:**
   Edit `config.yaml` and add your API keys for the LLM providers you want to use.

## Quick Start

### Option 1: Use the Interactive Launcher
```bash
python run_chroma_arena.py
```

This will guide you through the entire process with a menu-driven interface.

### Option 2: Use Command Line

1. **Initialize the system:**
   ```bash
   python main.py init
   ```

2. **Build RAG systems:**
   ```bash
   python main.py build
   ```

3. **Generate questions:**
   ```bash
   python main.py questions
   ```

4. **Generate answers:**
   ```bash
   python main.py answers
   ```

5. **Launch evaluation UI:**
   ```bash
   streamlit run eval_ui.py
   ```

### Option 3: Run Complete Pipeline
```bash
python main.py run-all
```

This will execute all steps automatically.

## Testing

Run the test suite to verify everything is working:
```bash
python test_chroma.py
```

## Directory Structure

After running the system, you'll have:
```
LLM Arena Chroma/
├── vector_stores/           # ChromaDB collections
│   ├── gemini-2.5-pro/
│   ├── openai-gpt-4o-mini/
│   └── claude-3-5-sonnet/
├── output/                  # Generated questions and answers
│   ├── curated_questions.json
│   ├── generated_answers.json
│   └── evaluation_results.db
└── data/source_docs/        # Your source documents
```

## Key Features

- **ChromaDB Integration**: Uses ChromaDB for vector storage and similarity search
- **Multi-LLM Support**: Evaluate OpenAI, Claude, and Gemini models
- **RAG Pipeline**: Complete pipeline from document ingestion to answer generation
- **Human Evaluation**: Web-based UI for human evaluation of LLM responses
- **Metadata Storage**: Rich metadata handling for document chunks

## Troubleshooting

### Common Issues

1. **Missing API keys**: Ensure all required API keys are in `config.yaml`
2. **No documents found**: Add PDF, TXT, or MD files to `data/source_docs/`
3. **ChromaDB errors**: Check that ChromaDB is properly installed
4. **Memory issues**: Reduce chunk size in `config.yaml` for large documents

### Getting Help

- Check the `README.md` for detailed documentation
- Review `CHROMA_VS_FAISS.md` for comparison with FAISS version
- Run `python test_chroma.py` to diagnose issues

## Next Steps

1. **Customize prompts**: Edit the prompts in `config.yaml`
2. **Add more documents**: Place additional documents in `data/source_docs/`
3. **Evaluate results**: Use the web UI to evaluate LLM responses
4. **Scale up**: Add more LLM providers or increase document collection

## Performance Tips

- **Chunk size**: Adjust `chunk_size` in config for optimal performance
- **Batch processing**: The system processes documents in batches
- **Memory usage**: ChromaDB uses more memory than FAISS but provides better metadata handling
- **Collection management**: ChromaDB collections can be versioned and managed independently 