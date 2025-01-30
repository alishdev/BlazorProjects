import argparse
import os
import tempfile
import uuid
import numpy as np
from pydub import AudioSegment
import whisper
import ssl
from qdrant_client import QdrantClient
from qdrant_client.http import models
from qdrant_client.http.models import Distance, VectorParams
import openai
import httpx

def main(mode):
    print('mode = ', mode)
    # Initialize OpenAI (make sure to set your API key in environment variables)
    openai.api_key = os.getenv("OPENAI_API_KEY")
    if not openai.api_key:
        raise ValueError("Please set OPENAI_API_KEY environment variable")

    # Initialize Qdrant client with increased timeout
    print('Connecting to Qdrant')
    client = QdrantClient(
        host="localhost", 
        port=6333,
        timeout=300  # Increase timeout to 5 minutes
    )

    collection_name = "audio_collection"

    if mode == 1:
        # Create collection and process files
        print("Mode 1: Creating collection and processing files")
        
        # Ensure collection exists with proper configuration
        try:
            client.get_collection(collection_name)
        except:
            # OpenAI text-embedding-ada-002 model produces vectors of size 1536
            client.create_collection(
                collection_name=collection_name,
                vectors_config=models.VectorParams(size=1536, distance=Distance.COSINE)
            )

        # Fix SSL certificate issues if any
        ssl._create_default_https_context = ssl._create_unverified_context

        # Load the Whisper model
        print('Loading Whisper model...')
        model = whisper.load_model("base")

        print('Loading mp3 files')
        # Process and upload mp3 files
        for file in os.listdir("mp3_files"):
            if file.endswith(".mp3"):
                process_file(file, model, client, collection_name)

    elif mode == 2:
        # Search and get AI answer
        print("Mode 2: Getting AI answer for question")
        question = "Which extracurricular activities are best for Georgetown?"
        search_results = search_audio_fragments(client, collection_name, "Georgetown extracurricular activities", limit=3)
        result = get_ai_answer(question, search_results)

        print("\nQuestion:", question)
        print("\nAnswer:", result["answer"])

        #print("\nResult:", result)

        if result["fragments"]:
            print("\nRelevant audio fragments:")
            for fragment in result["fragments"]:
                print(f"\nFile: {fragment['filename']}")
                print(f"Timestamp: {fragment['start']:.2f}s - {fragment['end']:.2f}s")
                print(f"Text: {fragment['text']}")

        if search_results:
            print("\nSource transcripts used:")
            for result in search_results:
                print(f"\nFrom file: {result.payload['filename']}")
                print(f"Similarity Score: {result.score}")
                print(f"Transcript excerpt: {result.payload['transcript'][:200]}...")
                #print(f"\nsearch_result_payload: {result.payload}")

def process_file(file, model, client, collection_name):
    """Process a single MP3 file and upload to Qdrant"""
    print(f"Processing {file}")
    mp3_path = os.path.join("mp3_files", file)
        
    # Convert to WAV temporarily for processing
    audio = AudioSegment.from_mp3(mp3_path)
    with tempfile.NamedTemporaryFile(suffix=".wav", delete=True) as temp_wav:
        audio.export(temp_wav.name, format="wav")
        
        # Transcribe audio using Whisper
        result = model.transcribe(temp_wav.name)
        transcript = result["text"]
        
        # Get segments with timestamps
        segments = result["segments"]
        segments_info = [
            {
                "text": seg["text"],
                "start": seg["start"],
                "end": seg["end"]
            }
            for seg in segments
        ]
        
        print('Getting OpenAI embedding')
        # Get embedding for the transcript
        embedding = get_embedding(transcript)
        
        print('Uploading to Qdrant')
        # Upload to Qdrant with transcript and segments in payload
        client.upsert(
            collection_name=collection_name,
            points=[
                {
                    "id": str(uuid.uuid4()),
                    "payload": {
                        "filename": file,
                        "file_path": mp3_path,
                        "transcript": transcript,
                        "segments": segments_info
                    },
                    "vector": embedding
                }
            ]
        )
        print(f"Uploaded {file} metadata to database")

def get_embedding(text):
    """Get OpenAI embedding for text by splitting into chunks and averaging"""
    # Split text into chunks of ~8000 characters (leaving some room for safety)
    chunk_size = 6000
    chunks = [text[i:i + chunk_size] for i in range(0, len(text), chunk_size)]
    
    # Get embeddings for each chunk
    embeddings = []
    for chunk in chunks:
        response = openai.Embedding.create(
            input=chunk,
            model="text-embedding-ada-002"
        )
        embeddings.append(response['data'][0]['embedding'])
    
    # Average the embeddings
    if not embeddings:
        raise ValueError("No embeddings generated")
        
    # Convert embeddings to numpy arrays and calculate mean
    embedding_arrays = np.array(embeddings)
    mean_embedding = np.mean(embedding_arrays, axis=0)
    
    return mean_embedding.tolist()

def search_audio_fragments(client, collection_name, search_term, limit=3):
    """Search for audio fragments using semantic search"""
    # Get embedding for the search term
    search_vector = get_embedding(search_term)
    
    # Search Qdrant using the embedding
    results = client.search(
        collection_name=collection_name,
        query_vector=search_vector,
        limit=limit,
        with_payload=True  # Make sure to include the full payload
    )
    return results

def get_ai_answer(question, search_results):
    """Get AI-generated answer based on search results"""
    if not search_results:
        return {"answer": "I do not have the information", "fragments": []}
    
    # Compile context from search results
    context = "Context from audio transcripts:\n"
    fragments_map = {}  # Map to store segments by file
    
    # Limit context size for each result
    max_chars_per_result = 2000
    for result in search_results:
        truncated_transcript = result.payload['transcript'][:max_chars_per_result]
        context += f"\nFrom {result.payload['filename']}:\n{truncated_transcript}"
        fragments_map[result.payload['filename']] = result.payload['segments']
    
    # Create prompt for OpenAI
    prompt = f"""Based only on the following context, answer the question. If the context doesn't contain relevant information, respond with "I do not have the information".
If you find relevant information, also identify at least two specific segments from the audio that support your answer.

Question: {question}

{context}

Format your response as follows:
ANSWER: [Your answer here]
SEGMENTS: [List the filenames and timestamp ranges that support your answer]
If no information is found, just respond with: ANSWER: I do not have the information"""

    response = openai.ChatCompletion.create(
        model="gpt-4-turbo",
        messages=[
            {"role": "system", "content": "You are a helpful assistant. Only use the provided context to answer questions. If you cannot find the specific information in the context, say 'I do not have the information'."},
            {"role": "user", "content": prompt}
        ],
        temperature=0,
        max_tokens=1000  # Limit response size
    )
    
    response_text = response.choices[0].message.content
    
    # Parse response to extract answer and segments
    if "I do not have the information" in response_text:
        return {"answer": "I do not have the information", "fragments": []}
    
    # Extract answer and segments from response
    answer_part = response_text.split("SEGMENTS:")[0].replace("ANSWER:", "").strip()
    segments_part = response_text.split("SEGMENTS:")[1].strip() if "SEGMENTS:" in response_text else ""
    
    # Process segments to create fragments list
    fragments = []
    for filename, segments in fragments_map.items():
        for segment in segments:
            if segment['text'] in segments_part:
                fragments.append({
                    "filename": filename,
                    "start": segment['start'],
                    "end": segment['end'],
                    "text": segment['text']
                })
    
    return {
        "answer": answer_part,
        "fragments": fragments[:3]  # Limit to top 3 most relevant fragments
    }

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Process audio files and query the database')
    parser.add_argument('mode', type=int, choices=[1, 2],
                      help='1: Create collection and process files, 2: Query the database')
    args = parser.parse_args()
    main(args.mode) 