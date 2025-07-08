import anthropic
from openai import OpenAI
from .base import LLMAdapter

class ClaudeAdapter(LLMAdapter):
    def __init__(self, config):
        super().__init__(config)
        # Configure the API key for Claude
        self.client = anthropic.Anthropic(api_key=config['api_key'])
        self.model = config['generation_model']
        self.embedding_model = config['embedding_model']
        
        # Use OpenAI for embeddings (Anthropic doesn't have embedding models)
        embedding_api_key = config.get('embedding_api_key', config['api_key'])
        self.embedding_client = OpenAI(api_key=embedding_api_key)
    
    def embed(self, text):
        """Generate embeddings using OpenAI API (since Anthropic doesn't have embedding models)."""
        try:
            response = self.embedding_client.embeddings.create(
                model=self.embedding_model,
                input=text
            )
            return response.data[0].embedding
        except Exception as e:
            print(f"[ERROR] Claude embedding failed: {e}")
            return None

    def generate(self, prompt, context=None, system_prompt=None):
        """Generate text using Claude model with RAG context."""
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
            
            # Prepare the API call parameters
            api_params = {
                "model": self.model,
                "max_tokens": 1000,
                "temperature": 0.7,
                "messages": [
                    {"role": "user", "content": full_prompt}
                ]
            }
            
            # Add system prompt as top-level parameter if provided
            if system_prompt:
                api_params["system"] = system_prompt
            
            # Generate content
            response = self.client.messages.create(**api_params)
            return response.content[0].text
        except Exception as e:
            print(f"[ERROR] Claude generation failed: {e}")
            return None 