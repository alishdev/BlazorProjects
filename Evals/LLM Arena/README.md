# LLM Arena

A suite for building, evaluating, and benchmarking Retrieval-Augmented Generation (RAG) systems across multiple LLM providers.

## Requirements
- Python 3.9+
- See `requirements.txt` for Python dependencies

## Setup

1. **Install dependencies:**
   ```bash
   python3 -m venv venv
   source venv/bin/activate
   pip install -r requirements.txt
   ```
2. **Set environment variables for your LLM API keys:**
   ```bash
   export GOOGLE_API_KEY=your_gemini_key
   export OPENAI_API_KEY=your_openai_key
   export ANTHROPIC_API_KEY=your_claude_key
   ```
3. **Configure your `config.yaml`:**
   - See the provided template for all required fields.
   - Place your source documents in the specified folder.

## Usage

Run the following commands in order:

1. **Build RAG systems:**
   ```bash
   python main.py build
   ```
2. **Generate and curate evaluation questions:**
   ```bash
   python main.py generate-questions
   ```
3. **Generate answers for each question:**
   ```bash
   python main.py generate-answers
   ```
4. **Launch the human evaluation UI:**
   ```bash
   ##streamlit run main.py launch-ui
   python main.py eval --ui gradio
   ```
   - Follow the UI to grade answers and save results to the database.

## Project Structure
- `main.py`: CLI entry point
- `config.py`: Config loader
- `rag_builder.py`: RAG system builder
- `question_gen.py`: Question generation
- `answer_gen.py`: Answer generation
- `eval_ui.py`: Human evaluation UI
- `db.py`: Database logic
- `adapters/`: LLM provider adapters
- `utils.py`: Utilities
- `config.yaml`: Configuration file

## Troubleshooting
- Ensure all API keys are set as environment variables before running the workflow.
- If you encounter missing package errors, re-run `pip install -r requirements.txt`.
- For PDF support, ensure `PyPDF2` is installed.
- If the UI does not launch, ensure you are running the correct Streamlit command.

## License
MIT 