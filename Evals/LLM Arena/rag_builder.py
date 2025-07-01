import os
import importlib
import numpy as np
import faiss
import pickle
from utils import load_documents, chunk_text

def build_rag_systems(config):
    """Build FAISS-based RAG systems for each LLM provider."""
    vector_store_path = config['vector_store_path']
    source_documents_path = config['source_documents_path']
    chunk_size = config.get('chunk_size', 512)
    chunk_overlap = config.get('chunk_overlap', 64)

    adapter_class_map = {
        "openai": "OpenAIAdapter",
        "gemini": "GeminiAdapter",
        "claude": "ClaudeAdapter"
    }

    for provider in config['llm_providers']:
        name = provider['name']
        print(f"[INFO] Building RAG for {name}...")
        provider_dir = os.path.join(vector_store_path, name)
        os.makedirs(provider_dir, exist_ok=True)
        # Load and chunk documents
        docs = load_documents(source_documents_path)
        print(f"[INFO] Loaded {len(docs)} documents from {source_documents_path}.")
        all_chunks = []
        for text, meta in docs:
            chunks = chunk_text(text, chunk_size, chunk_overlap)
            for idx, chunk in enumerate(chunks):
                chunk_meta = meta.copy()
                chunk_meta['chunk_id'] = idx
                all_chunks.append((chunk, chunk_meta))
        print(f"[INFO] Created {len(all_chunks)} text chunks for {name}.")
        # Dynamically import the correct adapter
        adapter_key = name.split('-')[0].lower()
        adapter_module = f"adapters.{adapter_key}_adapter"
        adapter_class = adapter_class_map.get(adapter_key)
        try:
            module = importlib.import_module(adapter_module, package=None)
            Adapter = getattr(module, adapter_class)
        except Exception as e:
            print(f"[ERROR] Could not import adapter for {name}: {e}")
            continue
        adapter = Adapter(provider)
        
        # Embed all chunks using the adapter
        embeddings = []
        for i, (chunk, meta) in enumerate(all_chunks):
            print(f"[INFO] Embedding chunk {i+1}/{len(all_chunks)} for {name}...")
            vector = adapter.embed(chunk)
            if vector is None:
                print(f"[ERROR] Failed to embed chunk {i+1} for {name}, skipping.")
                continue
            embeddings.append(vector)
        
        if not embeddings:
            print(f"[ERROR] No embeddings generated for {name}, skipping.")
            continue
            
        embeddings = np.stack(embeddings)
        embedding_dim = embeddings.shape[1]
        
        # Build and save FAISS index
        index = faiss.IndexFlatL2(embedding_dim)
        index.add(embeddings)
        faiss.write_index(index, os.path.join(provider_dir, "index.faiss"))
        print(f"[INFO] Saved FAISS index to {provider_dir}/index.faiss")
        # Save mapping file
        with open(os.path.join(provider_dir, "documents.pkl"), "wb") as f:
            pickle.dump(all_chunks, f)
        print(f"[INFO] Saved mapping file to {provider_dir}/documents.pkl")
    print("[INFO] RAG system build complete.") 