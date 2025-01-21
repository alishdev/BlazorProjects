import os

wav_files_dir = "wav_files"
print(f"Looking for WAV files in: {os.path.abspath(wav_files_dir)}")

for file in os.listdir("wav_files"):
    if file.endswith(".wav"):
        print(f"Processing {file}")