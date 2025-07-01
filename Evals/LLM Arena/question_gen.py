import os
import importlib
import pickle
import random
import json
from utils import deduplicate_questions

def generate_questions_for_provider(provider, adapter, vector_store_path, num_questions, generate_questions_prompt, output_data_path):
    """Generate questions for a specific provider."""
    name = provider['name']
    print(f"[INFO] Generating questions for {name}...")
    
    provider_dir = os.path.join(vector_store_path, name)
    mapping_path = os.path.join(provider_dir, "documents.pkl")
    
    if not os.path.exists(mapping_path):
        print(f"[WARN] Mapping file not found for {name}, skipping.")
        return []
    
    print(f"[INFO] Loading documents from {mapping_path}...")
    with open(mapping_path, "rb") as f:
        all_chunks = pickle.load(f)
    
    print(f"[INFO] Loaded {len(all_chunks)} chunks for {name}")
    
    # Combine all chunks into one text
    print(f"[INFO] Combining all chunks for {name}...")
    combined_text = ""
    for i, (chunk, meta) in enumerate(all_chunks):
        combined_text += f"\n\nChunk {i+1}:\n{chunk}"
    
    print(f"[INFO] Combined text length: {len(combined_text)} characters")
    
    # Format the prompt with the combined chunks and num_questions
    formatted_prompt = generate_questions_prompt.format(chunk=combined_text, num_questions=num_questions)
    
    print(f"[DEBUG] Sending request to {name} for all chunks...")
    
    # Generate questions using the LLM
    response = adapter.generate(formatted_prompt)
    
    if response:
        print(f"[DEBUG] Received response from {name} (length: {len(response)} chars)")
        
        # Parse the response to extract individual questions
        # Assuming the LLM returns a numbered list or bullet points
        lines = response.strip().split('\n')
        questions = []
        
        for line in lines:
            line = line.strip()
            # Skip empty lines
            if not line:
                continue
            # Remove numbering or bullet points and clean up
            if line.startswith(('1.', '2.', '3.', '4.', '5.', '6.', '7.', '8.', '9.', '0.', '-', '*', '•')):
                line = line[2:].strip() if line[1] == '.' else line[1:].strip()
            # Only add if it looks like a question
            if line.endswith('?') or '?' in line:
                questions.append(line)
        
        # If we got questions, save them and return
        if questions:
            print(f"[INFO] Extracted {len(questions)} questions from {name}")
            
            # Save questions to provider-specific file
            os.makedirs(output_data_path, exist_ok=True)
            provider_filename = f"questions_{name.replace('-', '_')}.json"
            provider_path = os.path.join(output_data_path, provider_filename)
            
            with open(provider_path, "w", encoding="utf-8") as f:
                json.dump({
                    "provider": name,
                    "num_questions_requested": num_questions,
                    "num_questions_generated": len(questions),
                    "questions": questions,
                    "raw_response": response
                }, f, indent=2, ensure_ascii=False)
            
            print(f"[INFO] Saved {len(questions)} questions from {name} to {provider_path}")
            return questions
        else:
            # Fallback: treat the entire response as one question if parsing fails
            print(f"[INFO] Using entire response as question for {name}")
            
            # Save the single question
            os.makedirs(output_data_path, exist_ok=True)
            provider_filename = f"questions_{name.replace('-', '_')}.json"
            provider_path = os.path.join(output_data_path, provider_filename)
            
            with open(provider_path, "w", encoding="utf-8") as f:
                json.dump({
                    "provider": name,
                    "num_questions_requested": num_questions,
                    "num_questions_generated": 1,
                    "questions": [response],
                    "raw_response": response
                }, f, indent=2, ensure_ascii=False)
            
            print(f"[INFO] Saved 1 question from {name} to {provider_path}")
            return [response]
    else:
        print(f"[WARN] Failed to generate questions for {name}")
        return []

def generate_and_curate_questions(config, skip_deduplication=False):
    """Generate and deduplicate evaluation questions."""
    print("[INFO] Starting question generation process...")
    
    vector_store_path = config['vector_store_path']
    output_data_path = config['output_data_path']
    num_questions = config.get('num_questions', 50)
    generate_questions_prompt = config.get('generate_questions_prompt', 
        "Based on the following text chunk, generate a clear and specific question that tests understanding of the key information presented. The question should be answerable from the content provided and should require comprehension rather than simple recall.\n\nText chunk: {chunk}\n\nQuestion:")
    
    print(f"[INFO] Configuration: {len(config['llm_providers'])} providers, {num_questions} questions per chunk")
    
    all_questions = []
    
    for provider_idx, provider in enumerate(config['llm_providers']):
        print(f"\n[INFO] Processing provider {provider_idx + 1}/{len(config['llm_providers'])}: {provider['name']}")
        
        # Dynamically import the correct adapter
        adapter_class_map = {
            "openai": "OpenAIAdapter",
            "gemini": "GeminiAdapter",
            "claude": "ClaudeAdapter"
        }
        
        adapter_key = provider['name'].split('-')[0].lower()
        adapter_module = f"adapters.{adapter_key}_adapter"
        adapter_class = adapter_class_map.get(adapter_key)
        
        print(f"[INFO] Loading adapter: {adapter_module}.{adapter_class}")
        
        try:
            module = importlib.import_module(adapter_module, package=None)
            Adapter = getattr(module, adapter_class)
        except Exception as e:
            print(f"[ERROR] Could not import adapter for {provider['name']}: {e}")
            continue
        
        adapter = Adapter(provider)
        print(f"[INFO] Successfully initialized {provider['name']} adapter")

        questions = generate_questions_for_provider(
            provider,
            adapter,
            vector_store_path, 
            num_questions, 
            generate_questions_prompt,
            output_data_path
        )
        all_questions.extend(questions)
        print(f"[INFO] Provider {provider['name']} completed. Total questions so far: {len(all_questions)}")
    
    print(f"\n[INFO] All providers completed. Generated {len(all_questions)} raw questions.")
    
    # Skip deduplication and curated file creation if requested
    if skip_deduplication:
        print("[INFO] Skipping deduplication and curated file creation as requested.")
        print("[INFO] Individual provider files have been saved. Question generation process completed!")
        return
    
    # Deduplicate
    print("[INFO] Starting deduplication process...")
    unique_questions = deduplicate_questions(all_questions)
    print(f"[INFO] {len(unique_questions)} unique questions after deduplication.")
    
    # Save
    print("[INFO] Saving questions to file...")
    os.makedirs(output_data_path, exist_ok=True)
    curated_path = os.path.join(output_data_path, "curated_questions.json")
    with open(curated_path, "w", encoding="utf-8") as f:
        json.dump(unique_questions, f, indent=2, ensure_ascii=False)
    print(f"[INFO] Saved curated questions to {curated_path}")
    print("[INFO] Question generation process completed successfully!") 