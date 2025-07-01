from openai import OpenAI
from .base import LLMAdapter

class OpenAIAdapter(LLMAdapter):
    def __init__(self, config):
        super().__init__(config)
        # Configure the API key and client
        self.client = OpenAI(api_key=config['api_key'])
        self.model = config['generation_model']
        self.embedding_model = config['embedding_model']
    
    def embed(self, text):
        """Generate embeddings using OpenAI API."""
        try:
            response = self.client.embeddings.create(
                model=self.embedding_model,
                input=text
            )
            return response.data[0].embedding
        except Exception as e:
            print(f"[ERROR] OpenAI embedding failed: {e}")
            return None

    def generate(self, prompt, context=None, system_prompt=None):
        """Generate text using OpenAI model with RAG context."""
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
            
            # Prepare messages with system prompt if provided
            messages = []
            if system_prompt:
                messages.append({"role": "system", "content": system_prompt})
            messages.append({"role": "user", "content": full_prompt})
            
            # Generate content using the new API format
            response = self.client.chat.completions.create(
                model=self.model,
                messages=messages,
                max_tokens=1000,
                temperature=0.7
            )
            return response.choices[0].message.content
        except Exception as e:
            print(f"[ERROR] OpenAI generation failed: {e}")
            return None 