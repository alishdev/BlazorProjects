import os
import importlib
import pickle
import json
import numpy as np
import faiss
import time
from utils import chunk_text

def generate_answers(config):
    """Generate answers for each question using all LLM providers."""
    vector_store_path = config['vector_store_path']
    output_data_path = config['output_data_path']
    curated_path = os.path.join(output_data_path, "curated_questions.json")
    if not os.path.exists(curated_path):
        print(f"[ERROR] curated_questions.json not found. Run generate_questions first.")
        return
    
    # Get generate answers prompt from config
    generate_answers_prompt = config.get('generate_answers_prompt', None)
    if generate_answers_prompt:
        print(f"[INFO] Using generate answers prompt: {generate_answers_prompt[:100]}...")
    else:
        print(f"[INFO] No generate answers prompt configured")
    
    with open(curated_path, "r", encoding="utf-8") as f:
        questions = json.load(f)
    
    print(f"[INFO] Processing {len(questions)} questions with {len(config['llm_providers'])} providers")
    print(f"[INFO] Total API calls expected: {len(questions) * len(config['llm_providers'])}")
    
    results = []
    adapter_class_map = {
        "openai": "OpenAIAdapter",
        "gemini": "GeminiAdapter",
        "claude": "ClaudeAdapter"
    }
    
    total_calls = len(questions) * len(config['llm_providers'])
    current_call = 0
    
    for q_idx, question in enumerate(questions):
        print(f"\n[INFO] Processing question {q_idx+1}/{len(questions)}: {question[:50]}...")
        entry = {
            "question_id": f"q_{q_idx+1:03d}",
            "question_text": question,
            "answers": {}
        }
        for provider in config['llm_providers']:
            name = provider['name']
            current_call += 1
            print(f"[INFO] Call {current_call}/{total_calls}: {name}")
            
            provider_dir = os.path.join(vector_store_path, name)
            index_path = os.path.join(provider_dir, "index.faiss")
            mapping_path = os.path.join(provider_dir, "documents.pkl")
            if not (os.path.exists(index_path) and os.path.exists(mapping_path)):
                print(f"[WARN] Missing index or mapping for {name}, skipping.")
                continue
            # Load FAISS index and mapping
            index = faiss.read_index(index_path)
            with open(mapping_path, "rb") as f:
                all_chunks = pickle.load(f)
            
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
            
            # Embed question using the adapter
            print(f"[INFO] Embedding question for {name}...")
            start_time = time.time()
            vector = adapter.embed(question)
            embed_time = time.time() - start_time
            print(f"[INFO] Embedding completed in {embed_time:.2f}s")
            
            if vector is None:
                print(f"[ERROR] Failed to embed question for {name}, skipping.")
                continue
            
            # Convert to numpy array and ensure correct type
            vector = np.array(vector, dtype='float32')
            
            # Retrieve top 3 chunks
            print(f"[INFO] Retrieving context for {name}...")
            D, I = index.search(vector.reshape(1, -1), 3)
            retrieved = [all_chunks[i][0] for i in I[0] if i < len(all_chunks)]
            context = "\n".join(retrieved)
            print(f"[INFO] Retrieved {len(retrieved)} chunks for {name}")
            
            # Generate answer using the adapter with RAG context and generate answers prompt
            print(f"[INFO] Generating answer for {name}...")
            start_time = time.time()
            answer = adapter.generate(question, context, generate_answers_prompt)
            gen_time = time.time() - start_time
            print(f"[INFO] Generation completed in {gen_time:.2f}s")
            
            if answer is None:
                print(f"[ERROR] Failed to generate answer for {name}, using fallback.")
                answer = f"[ERROR] Failed to generate answer for '{question[:40]}...' from {name}"
            
            entry["answers"][name] = answer
            print(f"[INFO] Completed {name} for question {q_idx+1}")
        
        results.append(entry)
        print(f"[INFO] Completed question {q_idx+1}/{len(questions)}")
    
    # Save
    gen_path = os.path.join(output_data_path, "generated_answers.json")
    with open(gen_path, "w", encoding="utf-8") as f:
        json.dump(results, f, indent=2, ensure_ascii=False)
    print(f"[INFO] Saved generated answers to {gen_path}") 