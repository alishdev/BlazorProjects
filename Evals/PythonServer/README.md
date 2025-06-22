# Evals FastAPI Server

A FastAPI application for evaluation services.

## Setup

1. **Navigate to the PythonServer directory**:
```bash
cd /Users/macmyths/BlazorProjects/Evals/PythonServer
```

2. Create a virtual environment (recommended):
```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

3. Install dependencies:
```bash
pip install -r requirements.txt
```

4. **Use the most reliable method**:
   ```bash
   python main.py
   ```

5. **For development with auto-reload**:
   ```bash
   uvicorn main:app --reload --host 0.0.0.0 --port 8000
   ``` 

************
had to run because of the bug in openai
pip install openai==1.55.3 httpx==0.27.2 --force-reinstall

curl calls:
curl -X POST "http://localhost:8000/askllm" -H "Content-Type: application/json" -d '{"llm": "gemini", "prompt": "Explain blockchain technology in one sentence", "model": "gemini-2.0-flash"}'
curl -X POST "http://localhost:8000/askllm" -H "Content-Type: application/json" -d '{"llm": "openai", "prompt": "Explain blockchain technology in one sentence", "model": "gpt-4o-2024-08-06"}'
curl -X POST "http://localhost:8000/askllm" -H "Content-Type: application/json" -d '{"llm": "anthropic", "prompt": "Explain blockchain technology in one sentence", "model": "claude-3-7-sonnet-latest"}'
