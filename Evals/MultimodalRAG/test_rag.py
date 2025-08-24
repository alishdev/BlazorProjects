#!/usr/bin/env python3
"""
Test script for the RAG system
Run this to verify your setup is working correctly
"""

import os
import sys
import tempfile
import shutil
from pathlib import Path

def create_test_pdf():
    """Create a simple test PDF file for testing"""
    try:
        # Try to create a simple text file first (easier for testing)
        test_content = """
        This is a test document for the RAG system.
        
        It contains information about machine learning and artificial intelligence.
        Machine learning is a subset of artificial intelligence that focuses on algorithms
        that can learn and make predictions from data.
        
        Some key concepts include:
        - Supervised learning
        - Unsupervised learning
        - Neural networks
        - Deep learning
        
        This document will be used to test the document processing, embedding generation,
        and vector search capabilities of the RAG system.
        """
        
        # Create a temporary directory
        test_dir = Path("test_documents")
        test_dir.mkdir(exist_ok=True)
        
        # Create a test text file (since creating PDFs programmatically is complex)
        test_file = test_dir / "test_document.txt"
        with open(test_file, 'w') as f:
            f.write(test_content)
        
        print(f"✅ Created test document: {test_file}")
        return str(test_dir)
        
    except Exception as e:
        print(f"❌ Error creating test document: {e}")
        return None

def test_imports():
    """Test if all required modules can be imported"""
    print("🔍 Testing imports...")
    
    try:
        import psycopg2
        print("✅ psycopg2 imported successfully")
    except ImportError as e:
        print(f"❌ psycopg2 import failed: {e}")
        return False
    
    try:
        import pgvector
        print("✅ pgvector imported successfully")
    except ImportError as e:
        print(f"❌ pgvector import failed: {e}")
        return False
    
    try:
        import jinaai
        print("✅ jinaai imported successfully")
    except ImportError as e:
        print(f"❌ jinaai import failed: {e}")
        return False
    
    try:
        import PyPDF2
        print("✅ PyPDF2 imported successfully")
    except ImportError as e:
        print(f"❌ PyPDF2 import failed: {e}")
        return False
    
    try:
        from dotenv import load_dotenv
        print("✅ python-dotenv imported successfully")
    except ImportError as e:
        print(f"❌ python-dotenv import failed: {e}")
        return False
    
    return True

def test_config():
    """Test configuration loading"""
    print("\n🔍 Testing configuration...")
    
    try:
        from config import Config
        print("✅ Configuration loaded successfully")
        
        # Check required values
        if Config.JINA_API_KEY:
            print("✅ JINA_API_KEY is set")
        else:
            print("⚠️  JINA_API_KEY is not set (will need to be set in .env)")
        
        print(f"✅ Vector dimension: {Config.VECTOR_DIMENSION}")
        print(f"✅ Chunk size: {Config.CHUNK_SIZE}")
        print(f"✅ Chunk overlap: {Config.CHUNK_OVERLAP}")
        
        return True
        
    except Exception as e:
        print(f"❌ Configuration test failed: {e}")
        return False

def test_database_connection():
    """Test database connection"""
    print("\n🔍 Testing database connection...")
    
    try:
        from database import DatabaseManager
        db = DatabaseManager()
        print("✅ Database connection successful")
        
        # Test basic operations
        count = db.get_document_count()
        print(f"✅ Document count query successful: {count}")
        
        db.close()
        return True
        
    except Exception as e:
        print(f"❌ Database connection test failed: {e}")
        print("   Make sure PostgreSQL is running and pgvector extension is installed")
        return False

def test_embeddings():
    """Test Jina AI embeddings"""
    print("\n🔍 Testing Jina AI embeddings...")
    
    try:
        from embeddings import EmbeddingsManager
        embeddings_manager = EmbeddingsManager()
        print("✅ Jina AI client initialized")
        
        # Test embedding generation
        test_text = "This is a test sentence for embedding generation."
        embedding = embeddings_manager.generate_embedding(test_text)
        
        print(f"✅ Embedding generated successfully")
        print(f"✅ Embedding shape: {embedding.shape}")
        print(f"✅ Embedding dimension: {embedding.shape[0]}")
        
        # Validate dimension
        embeddings_manager.validate_embedding_dimension(embedding)
        print("✅ Embedding dimension validation passed")
        
        return True
        
    except Exception as e:
        print(f"❌ Embeddings test failed: {e}")
        if "JINA_API_KEY is required" in str(e):
            print("   Make sure to set JINA_API_KEY in your .env file")
        return False

def test_document_processing():
    """Test document processing"""
    print("\n🔍 Testing document processing...")
    
    try:
        from document_processor import DocumentProcessor
        processor = DocumentProcessor()
        print("✅ Document processor initialized")
        
        # Test text chunking
        test_text = "This is a test document. " * 100  # Create long text
        chunks = processor.chunk_text(test_text)
        
        print(f"✅ Text chunking successful: {len(chunks)} chunks created")
        print(f"✅ First chunk length: {len(chunks[0])}")
        
        return True
        
    except Exception as e:
        print(f"❌ Document processing test failed: {e}")
        return False

def cleanup_test_files(test_dir):
    """Clean up test files"""
    try:
        if test_dir and os.path.exists(test_dir):
            shutil.rmtree(test_dir)
            print(f"🧹 Cleaned up test directory: {test_dir}")
    except Exception as e:
        print(f"⚠️  Warning: Could not clean up test directory: {e}")

def main():
    """Run all tests"""
    print("🚀 RAG System Test Suite")
    print("=" * 50)
    
    test_dir = None
    all_tests_passed = True
    
    try:
        # Test 1: Imports
        if not test_imports():
            all_tests_passed = False
        
        # Test 2: Configuration
        if not test_config():
            all_tests_passed = False
        
        # Test 3: Document processing
        if not test_document_processing():
            all_tests_passed = False
        
        # Test 4: Database connection
        if not test_database_connection():
            all_tests_passed = False
        
        # Test 5: Embeddings
        if not test_embeddings():
            all_tests_passed = False
        
        # Test 6: Create test documents
        test_dir = create_test_pdf()
        if test_dir:
            print(f"\n✅ Test documents created in: {test_dir}")
            print("   You can now test the full RAG system with:")
            print(f"   python main.py {test_dir}")
        
    except Exception as e:
        print(f"\n❌ Test suite failed with error: {e}")
        all_tests_passed = False
    
    finally:
        # Cleanup
        if test_dir:
            print("\n🧹 Cleaning up test files...")
            cleanup_test_files(test_dir)
    
    # Summary
    print("\n" + "=" * 50)
    if all_tests_passed:
        print("🎉 All tests passed! Your RAG system is ready to use.")
        print("\nNext steps:")
        print("1. Set your JINA_API_KEY in the .env file")
        print("2. Prepare your PDF documents")
        print("3. Run: python main.py /path/to/your/documents")
    else:
        print("❌ Some tests failed. Please check the errors above.")
        print("\nCommon issues:")
        print("- PostgreSQL not running or pgvector not installed")
        print("- Missing environment variables")
        print("- Network connectivity issues")
        print("- Missing Python dependencies")
    
    print("=" * 50)

if __name__ == "__main__":
    main()
