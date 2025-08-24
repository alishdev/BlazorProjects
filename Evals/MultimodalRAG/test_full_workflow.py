#!/usr/bin/env python3
"""
Test script for the complete RAG workflow
This demonstrates the full system: document processing, embedding generation, storage, and search
"""

import os
import tempfile
from rag_system import RAGSystem
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def create_test_document():
    """Create a test document with sample content"""
    test_content = """
    Machine Learning Fundamentals
    
    Machine learning is a subset of artificial intelligence that focuses on algorithms 
    that can learn and make predictions from data without being explicitly programmed. 
    There are three main types of machine learning:
    
    1. Supervised Learning: The algorithm learns from labeled training data to make 
       predictions on new, unseen data. Examples include classification and regression.
    
    2. Unsupervised Learning: The algorithm finds hidden patterns in data without 
       labeled examples. Examples include clustering and dimensionality reduction.
    
    3. Reinforcement Learning: The algorithm learns by interacting with an environment 
       and receiving rewards or penalties for actions.
    
    Deep learning is a subset of machine learning that uses neural networks with multiple 
    layers to model complex patterns in data. It has been particularly successful in 
    computer vision, natural language processing, and speech recognition.
    
    The key advantages of machine learning include:
    - Ability to handle large amounts of data
    - Automatic pattern recognition
    - Continuous improvement with more data
    - Scalability across different domains
    
    Common applications of machine learning include:
    - Recommendation systems
    - Fraud detection
    - Medical diagnosis
    - Autonomous vehicles
    - Natural language processing
    """
    
    # Create a temporary file
    with tempfile.NamedTemporaryFile(mode='w', suffix='.txt', delete=False) as f:
        f.write(test_content)
        temp_file = f.name
    
    return temp_file

def test_full_rag_workflow():
    """Test the complete RAG workflow"""
    print("🚀 Testing Complete RAG Workflow")
    print("=" * 50)
    
    try:
        # Initialize RAG system
        rag = RAGSystem()
        print("✅ RAG system initialized")
        
        # Get initial stats
        stats = rag.get_system_stats()
        print(f"\n📊 Initial system stats:")
        print(f"  Total documents: {stats['total_documents']}")
        print(f"  Vector dimensions: {stats['database_vector_dimension']}")
        print(f"  Embedding model: {stats['embedding_model']}")
        
        # Create test document
        test_file = create_test_document()
        test_dir = os.path.dirname(test_file)
        print(f"\n📄 Created test document: {test_file}")
        
        # Upload document
        print(f"\n📚 Uploading test document...")
        upload_result = rag.upload_documents(test_dir)
        
        if upload_result["status"] == "success":
            print("✅ Document upload successful!")
            print(f"  Chunks processed: {upload_result['chunks_processed']}")
            print(f"  Chunks stored: {upload_result['chunks_stored']}")
            print(f"  Chunks skipped: {upload_result['chunks_skipped']}")
        else:
            print(f"❌ Upload failed: {upload_result.get('message', 'Unknown error')}")
            return False
        
        # Get updated stats
        stats = rag.get_system_stats()
        print(f"\n📊 Updated system stats:")
        print(f"  Total documents: {stats['total_documents']}")
        
        # Test search functionality
        print(f"\n🔍 Testing search functionality...")
        
        # Test different queries
        test_queries = [
            "machine learning types",
            "deep learning neural networks",
            "supervised learning examples",
            "reinforcement learning environment"
        ]
        
        for query in test_queries:
            print(f"\n  Searching for: '{query}'")
            results = rag.search_documents(query, top_k=2)
            
            if results:
                print(f"    Found {len(results)} relevant documents:")
                for i, result in enumerate(results, 1):
                    print(f"      {i}. {result['filename']} (similarity: {result['similarity_score']:.4f})")
                    print(f"         Content: {result['content'][:100]}...")
            else:
                print(f"    No relevant documents found")
        
        # Clean up
        rag.close()
        os.unlink(test_file)
        
        print(f"\n🎉 Complete RAG workflow test successful!")
        return True
        
    except Exception as e:
        logger.error(f"Error during workflow test: {e}")
        print(f"❌ Workflow test failed: {e}")
        return False

def main():
    """Main test function"""
    print("🚀 Complete RAG System Test")
    print("=" * 50)
    
    success = test_full_rag_workflow()
    
    print("\n" + "=" * 50)
    if success:
        print("🎉 All tests passed! Your RAG system is fully operational.")
        print("\nYou can now:")
        print("1. Upload your own PDF documents")
        print("2. Search through your knowledge base")
        print("3. Build applications on top of this RAG system")
    else:
        print("❌ Some tests failed. Please check the errors above.")
    
    print("=" * 50)

if __name__ == "__main__":
    main()
