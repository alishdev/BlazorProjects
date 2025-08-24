import requests
import numpy as np
from config import Config
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class EmbeddingsManager:
    def __init__(self):
        if not Config.JINA_API_KEY:
            raise ValueError("JINA_API_KEY is required. Please set it in your .env file.")
        
        # Jina AI embeddings API endpoint
        self.api_url = "https://api.jina.ai/v1/embeddings"
        self.headers = {
            "Authorization": f"Bearer {Config.JINA_API_KEY}",
            "Content-Type": "application/json",
            "Accept": "application/json"
        }
        logger.info("Jina AI embeddings client initialized successfully")
    
    def truncate_embedding(self, embedding: np.ndarray) -> np.ndarray:
        """Truncate embedding from 2028 to 2000 dimensions for PostgreSQL storage"""
        if embedding.shape[0] > Config.DB_VECTOR_DIMENSION:
            truncated = embedding[:Config.DB_VECTOR_DIMENSION]
            logger.debug(f"Truncated embedding from {embedding.shape[0]} to {truncated.shape[0]} dimensions")
            return truncated
        return embedding
    
    def generate_embedding(self, text):
        """Generate embedding for a given text using Jina AI embeddings API"""
        try:
            payload = {
                "model": Config.JINA_MODEL_NAME,
                "input": [text]
                # No dimensions parameter - get full 2028 dimensions
            }
            
            response = requests.post(
                self.api_url,
                headers=self.headers,
                json=payload
            )
            
            if response.status_code != 200:
                raise Exception(f"API request failed with status {response.status_code}: {response.text}")
            
            # Extract the embedding vector
            data = response.json()
            embedding = data["data"][0]["embedding"]
            
            # Convert to numpy array
            embedding_array = np.array(embedding, dtype=np.float32)
            
            logger.debug(f"Generated embedding with shape: {embedding_array.shape}")
            return embedding_array
            
        except Exception as e:
            logger.error(f"Error generating embedding: {e}")
            raise
    
    def generate_embeddings_batch(self, texts, batch_size=10):
        """Generate embeddings for multiple texts in batches"""
        embeddings = []
        
        for i in range(0, len(texts), batch_size):
            batch = texts[i:i + batch_size]
            try:
                payload = {
                    "model": Config.JINA_MODEL_NAME,
                    "input": batch
                    # No dimensions parameter - get full 2028 dimensions
                }
                
                response = requests.post(
                    self.api_url,
                    headers=self.headers,
                    json=payload
                )
                
                if response.status_code != 200:
                    raise Exception(f"API request failed with status {response.status_code}: {response.text}")
                
                data = response.json()
                batch_embeddings = [np.array(item["embedding"], dtype=np.float32) 
                                  for item in data["data"]]
                embeddings.extend(batch_embeddings)
                
                logger.info(f"Processed batch {i//batch_size + 1}/{(len(texts) + batch_size - 1)//batch_size}")
                
            except Exception as e:
                logger.error(f"Error processing batch {i//batch_size + 1}: {e}")
                # Add zero vectors for failed embeddings to maintain alignment
                embeddings.extend([np.zeros(Config.VECTOR_DIMENSION, dtype=np.float32)] * len(batch))
        
        return embeddings
    
    def validate_embedding_dimension(self, embedding):
        """Validate that embedding has the correct dimension"""
        if embedding.shape[0] != Config.VECTOR_DIMENSION:
            raise ValueError(f"Embedding dimension mismatch. Expected {Config.VECTOR_DIMENSION}, got {embedding.shape[0]}")
        return True
    
    def prepare_embedding_for_db(self, embedding):
        """Prepare embedding for database storage by truncating to 2000 dimensions"""
        return self.truncate_embedding(embedding)
