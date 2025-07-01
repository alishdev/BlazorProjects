import google.generativeai as genai
from .base import LLMAdapter
import numpy as np

class GeminiAdapter(LLMAdapter):
    def __init__(self, config):
        super().__init__(config)
        # Configure the API key
        genai.configure(api_key=config['api_key'])
        # Get the model
        self.model = genai.GenerativeModel(config['generation_model'])
        self.embedding_model = config['embedding_model']
    
    def embed(self, text):
        # STUB: Return a random vector (rollback to previous behavior)
        embedding_dim = 768  # or whatever dimension was used previously
        return np.random.rand(embedding_dim).astype('float32')

    def generate(self, prompt, context=None, system_prompt=None):
        """Generate text using Gemini model with RAG context."""
        try:
            # Create a RAG prompt with context if provided
            if context:
                full_prompt = f"""Based on the following information, please answer the question:

Information:
{context}

Question: {prompt}

Please provide a comprehensive answer based on the information provided above."""
            else:
                full_prompt = prompt
            
            # For Gemini, we need to create a new model instance with system instruction if provided
            if system_prompt:
                model_with_system = genai.GenerativeModel(
                    self.config['generation_model'],
                    system_instruction=system_prompt
                )
                response = model_with_system.generate_content(full_prompt)
            else:
                response = self.model.generate_content(full_prompt)
            
            return response.text
        except Exception as e:
            print(f"[ERROR] Gemini generation failed: {e}")
            return None 