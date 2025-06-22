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