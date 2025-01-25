import os
import tempfile
import uuid
import librosa
import numpy as np
from pydub import AudioSegment
import whisper
import ssl
from qdrant_client import QdrantClient
from qdrant_client.http import models
from qdrant_client.http.models import Distance, VectorParams
import httpx

# Initialize Qdrant client with increased timeout
print('Connecting to Qdrant')
client = QdrantClient(
    host="localhost", 
    port=6333,
    timeout=300  # Increase timeout to 5 minutes
)

collection_name = "audio_collection"  # You can change this name if needed

# Ensure collection exists with proper configuration
try:
    client.get_collection(collection_name)
except:
    client.create_collection(
        collection_name=collection_name,
        vectors_config=models.VectorParams(size=128, distance=Distance.COSINE)
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
        print(f"Processing {file}")
        
        # Load and convert MP3 to WAV temporarily for feature extraction
        mp3_path = os.path.join("mp3_files", file)
            
        # Convert to WAV temporarily for processing
        audio = AudioSegment.from_mp3(mp3_path)
        with tempfile.NamedTemporaryFile(suffix=".wav", delete=True) as temp_wav:
            audio.export(temp_wav.name, format="wav")
            
            # Load audio file and extract features
            y, sr = librosa.load(temp_wav.name)
            
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
                            "file_path": mp3_path,  # Store the file path instead of binary data
                            "transcript": transcript,
                            "segments": segments_info
                        },
                        "vector": mfcc_mean.tolist()
                    }
                ]
            )
            print(f"Uploaded {file} metadata to database")

# Function to get MP3 file path from search results
def get_mp3_path_from_result(result):
    """Get the MP3 file path from a search result"""
    return result.payload['file_path']

# Add search function before using it
def search_audio_fragments(search_term, limit=3):
    """Search for audio fragments containing the search term"""
    return client.scroll(
        collection_name=collection_name,
        scroll_filter=models.Filter(
            must=[
                models.FieldCondition(
                    key="transcript",
                    match=models.MatchText(text=search_term)
                )
            ]
        ),
        limit=limit
    )[0]  # Return just the points, not the next page offset

# Now the search example will work
print('Searching for "georgetown" in database')
georgetown_results = search_audio_fragments("georgetown")
for i, result in enumerate(georgetown_results):
    print(f"\nFound 'georgetown' in file: {result.payload['filename']}")
    print(f"File path: {result.payload['file_path']}")
    print(f"Full Transcript: {result.payload['transcript']}")
    
    # Print segments containing the search term
    search_term_lower = "georgetown".lower()
    for segment in result.payload['segments']:
        if search_term_lower in segment['text'].lower():
            print(f"\nTimestamp: {segment['start']:.2f}s - {segment['end']:.2f}s")
            print(f"Segment: {segment['text']}") 