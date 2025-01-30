import os
import torch

print(os.environ)

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f"Using device: {device}")

wav_files_dir = "wav_files"
print(f"Looking for WAV files in: {os.path.abspath(wav_files_dir)}")

for file in os.listdir("wav_files"):
    if file.endswith(".wav"):
        print(f"Processing {file}")