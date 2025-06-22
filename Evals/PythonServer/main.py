from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional
import uvicorn
from ai_agents import AIAgents

# Create FastAPI app instance
app = FastAPI(
    title="Evals API",
    description="A FastAPI application for evaluation services",
    version="1.0.0"
)

# Initialize AIAgents instance
ai_agents = AIAgents()

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Configure this properly for production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Pydantic models
class EvaluationRequest(BaseModel):
    text: str
    criteria: Optional[List[str]] = None

class EvaluationResponse(BaseModel):
    score: float
    feedback: str
    criteria_met: List[str]

class AskLLMRequest(BaseModel):
    llm: str
    prompt: str
    model: Optional[str] = None

class AskLLMResponse(BaseModel):
    text: str

# Sample data
sample_evaluations = [
    {
        "id": 1,
        "text": "Sample evaluation 1",
        "score": 8.5,
        "feedback": "Good work overall"
    },
    {
        "id": 2,
        "text": "Sample evaluation 2", 
        "score": 7.2,
        "feedback": "Needs improvement"
    }
]

# Root endpoint
@app.get("/")
async def root():
    return {"message": "Welcome to Evals API", "version": "1.0.0"}

# Health check endpoint
@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "evals-api"}

# Get all evaluations
@app.get("/evaluations", response_model=List[dict])
async def get_evaluations():
    return sample_evaluations

# Get evaluation by ID
@app.get("/evaluations/{evaluation_id}")
async def get_evaluation(evaluation_id: int):
    for eval in sample_evaluations:
        if eval["id"] == evaluation_id:
            return eval
    raise HTTPException(status_code=404, detail="Evaluation not found")

# Create new evaluation
@app.post("/evaluations", response_model=EvaluationResponse)
async def create_evaluation(request: EvaluationRequest):
    # This is a simple example - in a real app, you'd implement actual evaluation logic
    score = len(request.text) / 10  # Simple scoring based on text length
    feedback = "Evaluation completed successfully"
    criteria_met = ["length", "content"] if len(request.text) > 10 else ["content"]
    
    return EvaluationResponse(
        score=min(score, 10.0),  # Cap at 10
        feedback=feedback,
        criteria_met=criteria_met
    )

# Update evaluation
@app.put("/evaluations/{evaluation_id}")
async def update_evaluation(evaluation_id: int, request: EvaluationRequest):
    for i, eval in enumerate(sample_evaluations):
        if eval["id"] == evaluation_id:
            sample_evaluations[i]["text"] = request.text
            return {"message": "Evaluation updated successfully"}
    raise HTTPException(status_code=404, detail="Evaluation not found")

# Delete evaluation
@app.delete("/evaluations/{evaluation_id}")
async def delete_evaluation(evaluation_id: int):
    for i, eval in enumerate(sample_evaluations):
        if eval["id"] == evaluation_id:
            deleted_eval = sample_evaluations.pop(i)
            return {"message": "Evaluation deleted successfully", "deleted": deleted_eval}
    raise HTTPException(status_code=404, detail="Evaluation not found")

# Ask LLM endpoint
@app.post("/askllm", response_model=AskLLMResponse)
async def ask_llm(request: AskLLMRequest):
    response_text = ai_agents.answer(request.llm, request.prompt, request.model)
    return AskLLMResponse(text=response_text)

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000) 