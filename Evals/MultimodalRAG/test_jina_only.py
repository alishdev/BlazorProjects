#!/usr/bin/env python3
"""
Test script for Jina AI embeddings only
This tests the embeddings functionality without requiring a database
"""

import os
import sys
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

def test_jina_embeddings():
    """Test Jina AI embeddings functionality"""
    print("🔍 Testing Jina AI embeddings...")
    
    # Check if API key is set
    api_key = os.getenv("JINA_API_KEY")
    if not api_key or api_key == "your_jina_api_key_here":
        print("❌ JINA_API_KEY not set in .env file")
        print("   Please set your actual Jina AI API key in the .env file")
        return False
    
    try:
        from embeddings import EmbeddingsManager
        embeddings_manager = EmbeddingsManager()
        print("✅ Jina AI embeddings client initialized")
        
        # Test embedding generation
        test_text = "This is a test sentence for embedding generation."
        print(f"   Testing with text: '{test_text}'")
        
        embedding = embeddings_manager.generate_embedding(test_text)
        
        print(f"✅ Embedding generated successfully")
        print(f"✅ Embedding shape: {embedding.shape}")
        print(f"✅ Embedding dimension: {embedding.shape[0]}")
        
        # Validate dimension
        embeddings_manager.validate_embedding_dimension(embedding)
        print("✅ Embedding dimension validation passed")
        
        # Test batch processing
        test_texts = [
            "First test sentence.",
            "Second test sentence.",
            "Third test sentence."
        ]
        print(f"\n   Testing batch processing with {len(test_texts)} texts...")
        
        batch_embeddings = embeddings_manager.generate_embeddings_batch(test_texts, batch_size=2)
        print(f"✅ Batch embeddings generated: {len(batch_embeddings)}")
        
        for i, emb in enumerate(batch_embeddings):
            print(f"   Text {i+1}: shape {emb.shape}, dimension {emb.shape[0]}")
        
        return True
        
    except Exception as e:
        print(f"❌ Embeddings test failed: {e}")
        return False

def main():
    """Main test function"""
    print("🚀 Jina AI Embeddings Test")
    print("=" * 40)
    
    success = test_jina_embeddings()
    
    print("\n" + "=" * 40)
    if success:
        print("🎉 Jina AI embeddings test passed!")
        print("\nNext steps:")
        print("1. Set up PostgreSQL with pgvector extension")
        print("2. Create the database and run setup_database.sql")
        print("3. Test the full RAG system")
    else:
        print("❌ Jina AI embeddings test failed")
        print("\nPlease check:")
        print("- Your JINA_API_KEY in the .env file")
        print("- Your internet connection")
        print("- Jina AI service status")
    
    print("=" * 40)

if __name__ == "__main__":
    main()
