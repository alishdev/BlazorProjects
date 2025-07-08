#!/usr/bin/env python3
"""
Test script for ChromaDB integration in LLM Arena
"""

import os
import sys
import chromadb
from chromadb.config import Settings

def test_chroma_connection():
    """Test basic ChromaDB connection and operations."""
    print("[TEST] Testing ChromaDB connection...")
    
    # Create a test directory
    test_dir = "./test_chroma"
    os.makedirs(test_dir, exist_ok=True)
    
    try:
        # Initialize ChromaDB client
        client = chromadb.PersistentClient(
            path=test_dir,
            settings=Settings(anonymized_telemetry=False)
        )
        print("[TEST] ✓ ChromaDB client initialized successfully")
        
        # Create a test collection
        collection_name = "test_collection"
        try:
            collection = client.get_collection(name=collection_name)
            print(f"[TEST] ✓ Using existing collection: {collection_name}")
        except:
            collection = client.create_collection(name=collection_name)
            print(f"[TEST] ✓ Created new collection: {collection_name}")
        
        # Test adding documents
        test_documents = [
            "This is a test document about machine learning.",
            "Another document about artificial intelligence.",
            "A third document about natural language processing."
        ]
        
        test_embeddings = [
            [0.1, 0.2, 0.3, 0.4, 0.5],  # Mock embeddings
            [0.2, 0.3, 0.4, 0.5, 0.6],
            [0.3, 0.4, 0.5, 0.6, 0.7]
        ]
        
        test_metadatas = [
            {"source": "test1", "type": "ml"},
            {"source": "test2", "type": "ai"},
            {"source": "test3", "type": "nlp"}
        ]
        
        test_ids = ["doc1", "doc2", "doc3"]
        
        collection.add(
            embeddings=test_embeddings,
            documents=test_documents,
            metadatas=test_metadatas,
            ids=test_ids
        )
        print("[TEST] ✓ Documents added successfully")
        
        # Test querying
        query_embedding = [0.15, 0.25, 0.35, 0.45, 0.55]
        results = collection.query(
            query_embeddings=[query_embedding],
            n_results=2
        )
        
        print(f"[TEST] ✓ Query successful, returned {len(results['documents'][0])} results")
        
        # Test collection count
        count = collection.count()
        print(f"[TEST] ✓ Collection count: {count}")
        
        # Clean up
        client.delete_collection(name=collection_name)
        print("[TEST] ✓ Collection deleted successfully")
        
        print("[TEST] ✓ All ChromaDB tests passed!")
        return True
        
    except Exception as e:
        print(f"[TEST] ✗ ChromaDB test failed: {e}")
        return False
    finally:
        # Clean up test directory
        import shutil
        if os.path.exists(test_dir):
            shutil.rmtree(test_dir)

def test_config_loading():
    """Test configuration loading."""
    print("\n[TEST] Testing configuration loading...")
    
    try:
        from config import load_config
        config = load_config()
        print("[TEST] ✓ Configuration loaded successfully")
        print(f"[TEST] ✓ Found {len(config['llm_providers'])} LLM providers")
        return True
    except Exception as e:
        print(f"[TEST] ✗ Configuration test failed: {e}")
        return False

def test_imports():
    """Test that all required modules can be imported."""
    print("\n[TEST] Testing module imports...")
    
    modules_to_test = [
        "chromadb",
        "numpy",
        "yaml",
        "click",
        "streamlit",
        "gradio"
    ]
    
    all_passed = True
    for module in modules_to_test:
        try:
            __import__(module)
            print(f"[TEST] ✓ {module} imported successfully")
        except ImportError as e:
            print(f"[TEST] ✗ Failed to import {module}: {e}")
            all_passed = False
    
    return all_passed

if __name__ == "__main__":
    print("=" * 50)
    print("LLM Arena ChromaDB Integration Test")
    print("=" * 50)
    
    # Run tests
    tests = [
        test_imports,
        test_config_loading,
        test_chroma_connection
    ]
    
    passed = 0
    total = len(tests)
    
    for test in tests:
        if test():
            passed += 1
    
    print("\n" + "=" * 50)
    print(f"Test Results: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 All tests passed! ChromaDB integration is working correctly.")
        sys.exit(0)
    else:
        print("❌ Some tests failed. Please check the errors above.")
        sys.exit(1) 