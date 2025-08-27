#!/usr/bin/env python3
"""
Performance Test Script for RAG System
This script will help identify bottlenecks in query processing
"""

import time
import psutil
import logging
from rag_system import RAGSystem
from embeddings import EmbeddingsManager
from database import DatabaseManager

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def test_embedding_generation_performance():
    """Test embedding generation performance"""
    print("🚀 Testing Embedding Generation Performance")
    print("=" * 50)
    
    emb_manager = EmbeddingsManager()
    
    # Test different text lengths
    test_texts = [
        "Short text",
        "This is a medium length text for testing embedding generation performance",
        "This is a much longer text that contains more words and should take longer to process. " * 10
    ]
    
    for i, text in enumerate(test_texts, 1):
        print(f"\n📝 Test {i}: Text length = {len(text)} characters")
        start_time = time.time()
        
        try:
            embedding = emb_manager.generate_embedding(text)
            generation_time = time.time() - start_time
            
            print(f"  ✅ Generated in: {generation_time:.4f}s")
            print(f"  📏 Embedding shape: {embedding.shape}")
            print(f"  🚀 Speed: {len(text)/generation_time:.0f} chars/second")
            
        except Exception as e:
            print(f"  ❌ Failed: {e}")

def test_database_search_performance():
    """Test database search performance"""
    print("\n🔍 Testing Database Search Performance")
    print("=" * 50)
    
    try:
        db_manager = DatabaseManager()
        
        # Test with different query embeddings
        test_queries = [
            "machine learning",
            "artificial intelligence",
            "deep learning neural networks",
            "natural language processing"
        ]
        
        for i, query in enumerate(test_queries, 1):
            print(f"\n🔍 Test {i}: Query = '{query}'")
            
            # Generate embedding for query
            emb_manager = EmbeddingsManager()
            start_time = time.time()
            
            try:
                query_embedding = emb_manager.generate_embedding(query)
                embedding_time = time.time() - start_time
                
                print(f"  ✅ Query embedding generated in: {embedding_time:.4f}s")
                
                # Test search performance
                search_start = time.time()
                results = db_manager.search_similar_documents(query_embedding, top_k=5)
                search_time = time.time() - search_start
                
                print(f"  🔍 Search completed in: {search_time:.4f}s")
                print(f"  📊 Results found: {len(results)}")
                
                # Calculate total query time
                total_time = embedding_time + search_time
                print(f"  ⏱️  Total query time: {total_time:.4f}s")
                
            except Exception as e:
                print(f"  ❌ Failed: {e}")
        
        db_manager.close()
        
    except Exception as e:
        print(f"❌ Database test failed: {e}")

def test_full_rag_performance():
    """Test full RAG system performance"""
    print("\n🎯 Testing Full RAG System Performance")
    print("=" * 50)
    
    try:
        rag = RAGSystem()
        
        # Test queries
        test_queries = [
            "machine learning algorithms",
            "deep learning applications",
            "AI research methods"
        ]
        
        for i, query in enumerate(test_queries, 1):
            print(f"\n🎯 Test {i}: Full RAG query = '{query}'")
            
            start_time = time.time()
            cpu_start = psutil.cpu_percent()
            memory_start = psutil.virtual_memory().used / 1024 / 1024
            
            try:
                results = rag.search_documents(query, top_k=5)
                total_time = time.time() - start_time
                cpu_end = psutil.cpu_percent()
                memory_end = psutil.virtual_memory().used / 1024 / 1024
                
                print(f"  ✅ Query completed in: {total_time:.4f}s")
                print(f"  📊 Results found: {len(results)}")
                print(f"  💾 Memory usage: {memory_end - memory_start:.2f} MB")
                print(f"  🖥️  CPU usage: {cpu_end - cpu_start:.1f}%")
                
                # Performance analysis
                if total_time > 5.0:
                    print(f"  ⚠️  SLOW: Query took {total_time:.1f}s (consider optimization)")
                elif total_time > 2.0:
                    print(f"  🟡 MEDIUM: Query took {total_time:.1f}s")
                else:
                    print(f"  🟢 FAST: Query took {total_time:.1f}s")
                
            except Exception as e:
                print(f"  ❌ Failed: {e}")
        
        rag.close()
        
    except Exception as e:
        print(f"❌ Full RAG test failed: {e}")

def test_database_connection_performance():
    """Test database connection and query performance"""
    print("\n🗄️ Testing Database Connection Performance")
    print("=" * 50)
    
    try:
        # Test connection time
        start_time = time.time()
        db_manager = DatabaseManager()
        connection_time = time.time() - start_time
        
        print(f"✅ Database connection: {connection_time:.4f}s")
        
        # Test simple query performance
        start_time = time.time()
        count = db_manager.get_document_count()
        query_time = time.time() - start_time
        
        print(f"✅ Document count query: {query_time:.4f}s")
        print(f"📊 Total documents: {count}")
        
        # Test index performance
        if count > 0:
            print(f"\n🔍 Testing index performance with {count} documents...")
            
            # Create a simple test embedding
            import numpy as np
            test_embedding = np.random.rand(2000).astype(np.float32)
            
            start_time = time.time()
            results = db_manager.search_similar_documents(test_embedding, top_k=5)
            search_time = time.time() - start_time
            
            print(f"✅ Vector search: {search_time:.4f}s")
            print(f"📊 Results: {len(results)}")
            
            # Performance analysis
            if search_time > 1.0:
                print(f"⚠️  SLOW: Vector search took {search_time:.1f}s")
                if count > 1000:
                    print(f"   💡 Consider optimizing HNSW index parameters for {count} documents")
            elif search_time > 0.5:
                print(f"🟡 MEDIUM: Vector search took {search_time:.1f}s")
            else:
                print(f"🟢 FAST: Vector search took {search_time:.1f}s")
        
        db_manager.close()
        
    except Exception as e:
        print(f"❌ Database performance test failed: {e}")

def main():
    """Run all performance tests"""
    print("🚀 RAG System Performance Analysis")
    print("=" * 60)
    
    # System info
    print(f"🖥️  CPU Cores: {psutil.cpu_count()}")
    print(f"💾 Total Memory: {psutil.virtual_memory().total / 1024 / 1024 / 1024:.1f} GB")
    print(f"🐍 Python: {psutil.Process().exe()}")
    
    # Run tests
    test_embedding_generation_performance()
    test_database_connection_performance()
    test_database_search_performance()
    test_full_rag_performance()
    
    print("\n" + "=" * 60)
    print("🎯 Performance Analysis Complete!")
    print("\n💡 Optimization Tips:")
    print("1. If embedding generation is slow: Consider batch processing")
    print("2. If database search is slow: Check HNSW index parameters")
    print("3. If memory usage is high: Consider smaller batch sizes")
    print("4. If CPU usage is high: Check for unnecessary computations")
    print("=" * 60)

if __name__ == "__main__":
    main()
