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

**Important**: Make sure your virtual environment is activated before running any commands!

### Troubleshooting Installation Issues

If you encounter compilation errors with pydantic-core (common on macOS), try these solutions:

#### Option 1: Use pre-compiled wheels
```bash
pip install --only-binary=all -r requirements.txt
```

#### Option 2: Install with specific platform
```bash
pip install --platform macosx_10_9_x86_64 --only-binary=:all: -r requirements.txt
```

#### Option 3: Use conda (if available)
```bash
conda install -c conda-forge fastapi uvicorn pydantic python-multipart
```

#### Option 4: Install Rust (if you want to compile from source)
```bash
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
source ~/.cargo/env
pip install -r requirements.txt
```

#### Option 5: Use alternative package manager
```bash
pip install fastapi uvicorn[standard] pydantic python-multipart --no-deps
pip install typing-extensions
```

## Running the Application

**⚠️ Always navigate to the correct directory and activate your virtual environment first:**
```bash
cd /Users/macmyths/BlazorProjects/Evals/PythonServer
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

### Method 1: Using Python directly (Simple, no auto-reload)
```bash
python main.py
```

### Method 2: Using Uvicorn with auto-reload (Recommended for development)
```bash
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

### Method 3: Using FastAPI CLI with auto-reload
```bash
fastapi run main.py --reload --host 0.0.0.0 --port 8000
```

The server will start on `http://localhost:8000`

**Note**: Methods 2 and 3 provide auto-reload functionality, which automatically restarts the server when you make code changes.

## API Documentation

Once the server is running, you can access:
- **Interactive API docs (Swagger UI)**: `http://localhost:8000/docs`
- **Alternative API docs (ReDoc)**: `http://localhost:8000/redoc`
- **OpenAPI schema**: `http://localhost:8000/openapi.json`

## Available Endpoints

- `GET /` - Welcome message
- `GET /health` - Health check
- `GET /evaluations` - Get all evaluations
- `GET /evaluations/{id}` - Get evaluation by ID
- `POST /evaluations` - Create new evaluation
- `PUT /evaluations/{id}` - Update evaluation
- `DELETE /evaluations/{id}` - Delete evaluation

## Example Usage

### Create an evaluation
```bash
curl -X POST "http://localhost:8000/evaluations" \
     -H "Content-Type: application/json" \
     -d '{"text": "This is a sample text for evaluation", "criteria": ["length", "content"]}'
```

### Get all evaluations
```bash
curl "http://localhost:8000/evaluations"
```

## Project Structure

```
Evals/
└── PythonServer/
    ├── main.py              # Main FastAPI application
    ├── requirements.txt     # Python dependencies
    └── README.md           # This file
```

## Features

- RESTful API with CRUD operations
- Automatic API documentation
- CORS middleware enabled
- Pydantic models for request/response validation
- Health check endpoint
- Sample data for testing

## Troubleshooting

If you get errors about missing files or wrong directories:

1. **Make sure you're in the correct directory**:
   ```bash
   cd /Users/macmyths/BlazorProjects/Evals/PythonServer
   ```

2. **Activate your virtual environment**:
   ```bash
   source venv/bin/activate
   ```

3. **Check which Python you're using**:
   ```bash
   which python
   which fastapi
   ```
   Both should point to your `venv/bin/` directory.

4. **Use the most reliable method**:
   ```bash
   python main.py
   ```

5. **For development with auto-reload**:
   ```bash
   uvicorn main:app --reload --host 0.0.0.0 --port 8000
   ``` 