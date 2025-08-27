import requests
import numpy as np
from config import Config
import logging
import time
import psutil

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
        """Truncate embedding from 2048 to 2000 dimensions for PostgreSQL storage"""
        if embedding.shape[0] > Config.DB_VECTOR_DIMENSION:
            truncated = embedding[:Config.DB_VECTOR_DIMENSION]
            logger.debug(f"Truncated embedding from {embedding.shape[0]} to {truncated.shape[0]} dimensions")
            return truncated
        return embedding
    
    def generate_embedding(self, text):
        """Generate embedding for a given text using Jina AI embeddings API"""
        start_time = time.time()
        cpu_start = psutil.cpu_percent()
        memory_start = psutil.virtual_memory().used / 1024 / 1024  # MB
        
        try:
            # Step 1: Prepare payload
            step1_start = time.time()
            payload = {
                "model": Config.JINA_MODEL_NAME,
                "input": [text]
                # No dimensions parameter - get full 2048 dimensions
            }
            step1_time = time.time() - step1_start
            
            # Step 2: Make API request
            step2_start = time.time()
            response = requests.post(
                self.api_url,
                headers=self.headers,
                json=payload
            )
            step2_time = time.time() - step2_start
            
            if response.status_code != 200:
                raise Exception(f"API request failed with status {response.status_code}: {response.text}")
            
            # Step 3: Process response
            step3_start = time.time()
            data = response.json()
            embedding = data["data"][0]["embedding"]
            
            # Convert to numpy array
            embedding_array = np.array(embedding, dtype=np.float32)
            step3_time = time.time() - step3_start
            
            # Calculate total time and resource usage
            total_time = time.time() - start_time
            cpu_end = psutil.cpu_percent()
            memory_end = psutil.virtual_memory().used / 1024 / 1024  # MB
            
            # Log performance metrics
            logger.info(f"🚀 Embedding Generation Performance Profile:")
            logger.info(f"  ⏱️  Total time: {total_time:.4f}s")
            logger.info(f"  📊 Step breakdown:")
            logger.info(f"     - Payload preparation: {step1_time:.6f}s ({step1_time/total_time*100:.1f}%)")
            logger.info(f"     - API request: {step2_time:.6f}s ({step2_time/total_time*100:.1f}%)")
            logger.info(f"     - Response processing: {step3_time:.6f}s ({step3_time/total_time*100:.1f}%)")
            logger.info(f"  💾 Memory usage: {memory_end - memory_start:.2f} MB")
            logger.info(f"  🖥️  CPU usage: {cpu_end - cpu_start:.1f}%")
            logger.info(f"  📏 Generated embedding shape: {embedding_array.shape}")
            
            return embedding_array
            
        except Exception as e:
            logger.error(f"Error generating embedding: {e}")
            raise
    
    def generate_embeddings_batch(self, texts, batch_size=10):
        """Generate embeddings for multiple texts in batches"""
        start_time = time.time()
        embeddings = []
        
        for i in range(0, len(texts), batch_size):
            batch = texts[i:i + batch_size]
            batch_start = time.time()
            
            try:
                payload = {
                    "model": Config.JINA_MODEL_NAME,
                    "input": batch
                    # No dimensions parameter - get full 2048 dimensions
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
                
                batch_time = time.time() - batch_start
                logger.info(f"Processed batch {i//batch_size + 1}/{(len(texts) + batch_size - 1)//batch_size} in {batch_time:.4f}s")
                
            except Exception as e:
                logger.error(f"Error processing batch {i//batch_size + 1}: {e}")
                # Add zero vectors for failed embeddings to maintain alignment
                embeddings.extend([np.zeros(Config.VECTOR_DIMENSION, dtype=np.float32)] * len(batch))
        
        total_time = time.time() - start_time
        logger.info(f"🚀 Batch embedding generation completed in {total_time:.4f}s for {len(texts)} texts")
        
        return embeddings
    
    def validate_embedding_dimension(self, embedding):
        """Validate that embedding has the correct dimension"""
        if embedding.shape[0] != Config.VECTOR_DIMENSION:
            raise ValueError(f"Embedding dimension mismatch. Expected {Config.VECTOR_DIMENSION}, got {embedding.shape[0]}")
        return True
    
    def prepare_embedding_for_db(self, embedding):
        """Prepare embedding for database storage by truncating to 2000 dimensions"""
        return self.truncate_embedding(embedding)
