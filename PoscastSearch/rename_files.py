import os
import re

def rename_files(directory='.'):
    """
    Rename files in the specified directory by removing the first 4 words.
    Words can be delimited by spaces or underscores.
    """
    # Get all files in the directory
    for filename in os.listdir(directory):
        # Skip directories
        if os.path.isdir(os.path.join(directory, filename)):
            continue
            
        # Split filename into name and extension
        name, ext = os.path.splitext(filename)
        
        # Split the name by both spaces and underscores
        words = re.split(r'[_\s]+', name)
        
        # If we have more than 4 words, create new filename
        if len(words) > 4:
            new_name = '_'.join(words[4:]) + ext
            old_path = os.path.join(directory, filename)
            new_path = os.path.join(directory, new_name)
            
            try:
                os.rename(old_path, new_path)
                print(f"Renamed: {filename} -> {new_name}")
            except Exception as e:
                print(f"Error renaming {filename}: {str(e)}")
        else:
            print(f"Skipping {filename}: Not enough words")

if __name__ == "__main__":
    # You can specify a different directory as an argument
    import sys
    directory = sys.argv[1] if len(sys.argv) > 1 else '.'
    rename_files(directory) 