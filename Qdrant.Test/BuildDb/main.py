#  build code that reads wav files from the folder and uploads then into a Qdrant vector database

import os
import qdrant_client
from qdrant_client import QdrantClient
from qdrant_client.http.models import Distance, VectorParams
import whisper  # Added for whisper model
import ssl
import certifi
import uuid  # Add this import at the top

print('Connecting to Qdrant')
client = QdrantClient(host="localhost", port=6333)

# Create collection for audio vectors if it doesn't exist
collection_name = "audio_collection"
if client.collection_exists(collection_name):
    client.delete_collection(collection_name)
    
client.create_collection(
    collection_name=collection_name,
    vectors_config=VectorParams(size=128, distance=Distance.COSINE),
)

# Import librosa for audio feature extraction
import librosa
import numpy as np

# Add this line before loading the whisper model
ssl._create_default_https_context = ssl._create_unverified_context

# Load whisper model
model = whisper.load_model("base")

print('Loading wav files')
# Process and upload wav files
for file in os.listdir("wav_files"):
    if file.endswith(".wav"):
        print(f"Processing {file}")
        
        # Load audio file and extract features
        audio_path = os.path.join("wav_files", file)
        y, sr = librosa.load(audio_path)
        
        # Transcribe audio using Whisper
        result = model.transcribe(audio_path)
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
        
        # Extract MFCC features
        mfcc = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=128)
        mfcc_mean = np.mean(mfcc, axis=1)
        
        print('Uploading to Qdrant')
        
        # Upload to Qdrant with transcript and segments in payload
        client.upsert(
            collection_name=collection_name,
            points=[
                {
                    "id": str(uuid.uuid4()),
                    "payload": {
                        "filename": file,
                        "transcript": transcript,
                        "segments": segments_info
                    },
                    "vector": mfcc_mean.tolist()
                }
            ]
        )
        print(f"Uploaded {file} to database")

# Add search function
def search_audio_fragments(search_term):
    print(f"Searching for '{search_term}' in database")
    results = client.scroll(
        collection_name=collection_name,
        scroll_filter=qdrant_client.http.models.Filter(
            must=[
                qdrant_client.http.models.FieldCondition(
                    key="transcript",
                    match=qdrant_client.http.models.MatchText(
                        text=search_term
                    )
                )
            ]
        ),
        limit=10
    )
    
    return results[0]

# Example usage
print('Searching for "canoe" in database')
canoe_results = search_audio_fragments("canoe")
for result in canoe_results:
    print(f"\nFound 'canoe' in file: {result.payload['filename']}")
    print(f"Full Transcript: {result.payload['transcript']}")
    
    # Print segments containing the search term
    search_term_lower = "canoe".lower()
    for segment in result.payload['segments']:
        if search_term_lower in segment['text'].lower():
            print(f"\nTimestamp: {segment['start']:.2f}s - {segment['end']:.2f}s")
            print(f"Segment: {segment['text']}")

print("\nDatabase search complete")

