import os
import argparse
import whisper
import ssl
import logging
import time
from datetime import timedelta

def setup_logging():
    """Set up logging configuration"""
    # Create logs directory if it doesn't exist
    os.makedirs('logs', exist_ok=True)
    
    # Set up logging format
    log_format = '%(asctime)s - %(levelname)s - %(message)s'
    
    # Configure logging to write to both file and console
    logging.basicConfig(
        level=logging.INFO,
        format=log_format,
        handlers=[
            logging.FileHandler('logs/transcribe.log'),
            logging.StreamHandler()
        ]
    )
    
    return logging.getLogger(__name__)

def list_directory(directory):
    try:
        # Check if directory exists
        if not os.path.exists(directory):
            logger.error(f"Directory not found: {directory}")
            return []
        
        # Get all MP3 files in directory with full paths
        file_paths = []
        for file in sorted(os.listdir(directory)):
            if file.endswith('.mp3'):
                full_path = os.path.join(directory, file)
                file_paths.append(full_path)
        
        return file_paths
        
    except Exception as e:
        logger.error(f"Error listing directory: {str(e)}")
        return []

#WHISPER_MODEL = "large-v2"
WHISPER_MODEL = "medium"

def transcribe(filepath):
    """
    Transcribe an audio file using Whisper and save to text file
    
    Args:
        filepath: Path to the audio file
        
    Returns:
        dict: Transcription result containing text and segments
    """
    try:
        # Check if file exists
        if not os.path.exists(filepath):
            logger.error(f"File not found: {filepath}")
            return None
        
        # Create SSL context that ignores certificate verification
        ssl._create_default_https_context = ssl._create_unverified_context
            
        logger.info(f"Loading Whisper {WHISPER_MODEL} model...")
        model = whisper.load_model(WHISPER_MODEL)
        
        # Start timing
        start_time = time.time()
        
        logger.info(f"Transcribing {filepath}...")
        result = model.transcribe(filepath)
        
        # Calculate elapsed time
        elapsed_time = time.time() - start_time
        elapsed_str = str(timedelta(seconds=int(elapsed_time)))
        logger.info(f"Transcription completed in {elapsed_str}")
        
        # Save transcription to text file
        if result and "text" in result:
            # Create output filename by replacing .mp3 with .txt
            output_file = os.path.splitext(filepath)[0] + "_" + WHISPER_MODEL + ".txt"
            
            # Save transcription to file
            try:
                with open(output_file, 'w', encoding='utf-8') as f:
                    f.write(result["text"])
                logger.info(f"Transcription saved to: {output_file}")
            except Exception as e:
                logger.error(f"Error saving transcription: {str(e)}")
        
        return result
        
    except Exception as e:
        logger.error(f"Error transcribing file: {str(e)}")
        return None

if __name__ == "__main__":
    # Set up logging
    logger = setup_logging()
    logger.info("Starting transcription tool...")
    
    # Track total processing time
    total_start_time = time.time()
    
    # Set up argument parser
    parser = argparse.ArgumentParser(description='Audio file transcription tool')
    parser.add_argument('--list', metavar='DIRECTORY', type=str, help='List MP3 files in the specified directory')
    parser.add_argument('--transcribe', metavar='PATH', type=str, help='Transcribe a file or all MP3 files in a directory')
    parser.add_argument('--concat', metavar='DIRPATH', type=str, help='Directory path to concatenate all files')
    
    # Parse arguments
    args = parser.parse_args()
    
    # Run appropriate function based on arguments
    if args.concat:
        try:
            with open('all_in.txt', 'w', encoding='utf-8') as outfile:
                for filename in os.listdir(args.concat):
                    if filename.endswith('.txt'):
                        filepath = os.path.join(args.concat, filename)
                        #logger.info(f"Processing {filename}")
                        if os.path.isfile(filepath):
                            with open(filepath, 'r', encoding='utf-8') as infile:
                                outfile.write(infile.read() + '\n')
            logger.info("All files concatenated to all_in.txt")
        except Exception as e:
            logger.error(f"Error concatenating files: {str(e)}")
    elif args.list:
        files = list_directory(args.list)
        if files:
            logger.info(f"\nFound {len(files)} MP3 files in {args.list}:")
            logger.info("-" * 50)
            for file_path in files:
                logger.info(file_path)
            logger.info("-" * 50)
    elif args.transcribe:
        if os.path.isfile(args.transcribe):
            # Single file transcription
            logger.info(f"\nTranscribing single file: {args.transcribe}")
            result = transcribe(args.transcribe)
        elif os.path.isdir(args.transcribe):
            # Directory transcription
            files = list_directory(args.transcribe)
            if files:
                logger.info(f"\nFound {len(files)} MP3 files to transcribe in {args.transcribe}")
                for file_path in files:
                    logger.info(f"\nProcessing: {file_path}")
                    logger.info("-" * 50)
                    result = transcribe(file_path)
            else:
                logger.warning(f"No MP3 files found in directory: {args.transcribe}")
        else:
            logger.error(f"Error: {args.transcribe} is neither a file nor a directory")
    else:
        logger.info("Usage:")
        logger.info("  List files: python transcribe.py --list 'directory_path'")
        logger.info("  Transcribe file: python transcribe.py --transcribe 'file_path'")
        logger.info("  Transcribe directory: python transcribe.py --transcribe 'directory_path'")
        logger.info("\nExamples:")
        logger.info("  python transcribe.py --list 'podcast_episodes'")
        logger.info("  python transcribe.py --transcribe 'podcast_episodes/episode1.mp3'")
        logger.info("  python transcribe.py --transcribe 'podcast_episodes'")
    
    # Log total processing time
    total_elapsed = time.time() - total_start_time
    total_elapsed_str = str(timedelta(seconds=int(total_elapsed)))
    logger.info(f"Transcription tool finished. Total time: {total_elapsed_str}") 