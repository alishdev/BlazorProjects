#!/usr/bin/env python3
"""
Sample RAG System using PostgreSQL, pgvector, and Jina AI embeddings
"""

import os
import sys
from rag_system import RAGSystem
import logging

# Set up logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

def main():
    """Main function to demonstrate RAG system usage"""
    
    # Check if PDF directory is provided as command line argument
    if len(sys.argv) < 2:
        print("Usage: python main.py <pdf_directory_path>")
        print("Example: python main.py ./documents")
        sys.exit(1)
    
    pdf_directory = sys.argv[1]
    
    # Check if directory exists
    if not os.path.exists(pdf_directory):
        print(f"Error: Directory '{pdf_directory}' does not exist.")
        sys.exit(1)
    
    if not os.path.isdir(pdf_directory):
        print(f"Error: '{pdf_directory}' is not a directory.")
        sys.exit(1)
    
    # Initialize RAG system
    try:
        rag_system = RAGSystem()
        print("✅ RAG system initialized successfully")
        
        # Get system stats before upload
        print("\n📊 System Statistics (before upload):")
        stats = rag_system.get_system_stats()
        for key, value in stats.items():
            print(f"  {key}: {value}")
        
        # Upload documents
        print(f"\n📚 Uploading documents from: {pdf_directory}")
        upload_result = rag_system.upload_documents(pdf_directory)
        
        if upload_result["status"] == "success":
            print("✅ Document upload completed successfully!")
            print(f"  Chunks processed: {upload_result['chunks_processed']}")
            print(f"  Chunks stored: {upload_result['chunks_stored']}")
            print(f"  Files processed: {upload_result['files_processed']}")
        else:
            print(f"❌ Document upload failed: {upload_result.get('message', 'Unknown error')}")
            return
        
        # Get updated system stats
        print("\n📊 System Statistics (after upload):")
        stats = rag_system.get_system_stats()
        for key, value in stats.items():
            print(f"  {key}: {value}")
        
        # Interactive search
        print("\n🔍 Interactive Search Mode")
        print("Enter your search queries (type 'quit' to exit):")
        
        while True:
            try:
                query = input("\nEnter search query: ").strip()
                
                if query.lower() in ['quit', 'exit', 'q']:
                    break
                
                if not query:
                    continue
                
                # Search for documents
                results = rag_system.search_documents(query, top_k=3)
                
                if results:
                    print(f"\n📄 Found {len(results)} relevant documents:")
                    for i, result in enumerate(results, 1):
                        print(f"\n--- Result {i} ---")
                        print(f"📁 File: {result['filename']}")
                        print(f"🔢 Chunk: {result['chunk_index']}")
                        print(f"📊 Similarity: {result['similarity_score']:.4f}")
                        print(f"📝 Content: {result['content'][:200]}...")
                else:
                    print("❌ No relevant documents found.")
                    
            except KeyboardInterrupt:
                print("\n\n👋 Goodbye!")
                break
            except Exception as e:
                print(f"❌ Error during search: {e}")
        
        # Clean up
        rag_system.close()
        print("\n✅ RAG system shutdown complete")
        
    except Exception as e:
        logger.error(f"Failed to initialize RAG system: {e}")
        print(f"❌ Error: {e}")
        print("\nPlease check:")
        print("1. Your .env file has the correct JINA_API_KEY")
        print("2. PostgreSQL is running and accessible")
        print("3. The pgvector extension is installed")
        print("4. Your database connection string is correct")
        sys.exit(1)

if __name__ == "__main__":
    main()
