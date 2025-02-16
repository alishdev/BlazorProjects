import os
import argparse
import logging
import time
import re
from summarize_podcast import PodcastSummarizer
from dotenv import load_dotenv
import google.generativeai as genai

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
            logging.FileHandler('logs/toc.log'),
            logging.StreamHandler()
        ]
    )
    
    return logging.getLogger(__name__)

def process_directory(directory):
    """
    Process all text files in the directory using PodcastSummarizer
    
    Args:
        directory: Path to the directory containing text files
        
    Returns:
        tuple: (total_input_tokens, total_output_tokens)
    """
    logger = setup_logging()
    logger.info(f"Processing directory: {directory}")
    
    # Initialize token counters
    total_input_tokens = 0
    total_output_tokens = 0
    
    try:
        # Create summarizer instance
        summarizer = PodcastSummarizer()
        
        # Get all text files in directory, excluding .sum files
        text_files = [f for f in sorted(os.listdir(directory)) 
                     if f.endswith('.txt') and not os.path.exists(os.path.join(directory, f[:-4] + '.sum'))]
        
        if not text_files:
            logger.warning(f"No text files found in directory: {directory}")
            return 0, 0
        
        logger.info(f"Found {len(text_files)} text files to process")
        
        # Process each file
        for i, filename in enumerate(text_files, 1):
            filepath = os.path.join(directory, filename)
            logger.info(f"\nProcessing file {i}/{len(text_files)}: {filename}")
            
            success, input_tokens, output_tokens = summarizer.process_file(filepath)
            
            if success and input_tokens and output_tokens:
                total_input_tokens += input_tokens
                total_output_tokens += output_tokens
                logger.info(f"File tokens - Input: {input_tokens}, Output: {output_tokens}")
            else:
                logger.error(f"Failed to process file: {filename}")
            
            # Add delay if there are more files to process
            if i < len(text_files):
                logger.info("Waiting 10 seconds before processing next file...")
                time.sleep(10)
        
        # Log total token usage
        logger.info("\nTotal token usage:")
        logger.info(f"Total input tokens: {total_input_tokens}")
        logger.info(f"Total output tokens: {total_output_tokens}")
        logger.info(f"Total combined tokens: {total_input_tokens + total_output_tokens}")
        
        return total_input_tokens, total_output_tokens
        
    except Exception as e:
        logger.error(f"Error processing directory: {str(e)}")
        return 0, 0

def create_toc(directory):
    """
    Create a table of contents by concatenating all .sum files
    
    Args:
        directory: Path to the directory containing .sum files
    """
    logger = setup_logging()
    logger.info(f"Creating table of contents from directory: {directory}")
    
    try:
        # Get all .sum files in directory
        sum_files = [f for f in sorted(os.listdir(directory)) if f.endswith('.sum')]
        
        if not sum_files:
            logger.warning(f"No .sum files found in directory: {directory}")
            return False
        
        logger.info(f"Found {len(sum_files)} summary files")
        
        # Create output file
        output_file = os.path.join(directory, "table_of_contents.txt")
        
        # Concatenate all summaries
        with open(output_file, 'w', encoding='utf-8') as outfile:
            for i, filename in enumerate(sum_files, 1):
                filepath = os.path.join(directory, filename)
                logger.info(f"Adding summary from: {filename}")
                
                # Add chapter header
                #outfile.write(f"Chapter {i}: {os.path.splitext(filename)[0]}\n")
                #outfile.write("-" * 50 + "\n")
                
                # Add summary content
                with open(filepath, 'r', encoding='utf-8') as infile:
                    content = infile.read()
                    # remove the first line
                    content = content.split('\n', 1)[1]
                    # remove the next line if it is empty
                    if content.startswith('\n'):
                        content = content.split('\n', 1)[1]
                    # remove the next line if it contains word episode (case insensitive    )
                    if re.search(r'Episode', content, re.IGNORECASE):
                        content = content.split('\n', 1)[1]
                    outfile.write(content)
                
                outfile.write("\n\n")
        
        logger.info(f"Table of contents created: {output_file}")
        return True
        
    except Exception as e:
        logger.error(f"Error creating table of contents: {str(e)}")
        return False

def toc_chapters(filename):
    """
    Create an AI-generated table of contents using Gemini
    
    Args:
        filename: Path to the file containing concatenated summaries
    """
    logger = setup_logging()
    logger.info(f"Creating top level table of contents from file: {filename}")
    
    try:
        # Check if file exists
        if not os.path.exists(filename):
            logger.error(f"File not found: {filename}")
            return False
        
        # Load environment variables and configure API
        load_dotenv()
        api_key = os.getenv('GOOGLE_API_KEY')
        if not api_key:
            logger.error("GOOGLE_API_KEY not found in environment variables")
            return False
            
        genai.configure(api_key=api_key)
        model = genai.GenerativeModel('gemini-2.0-flash')
        
        # Read the content file
        with open(filename, 'r', encoding='utf-8') as f:
            content = f.read()

        output_format = """{
        "TOC": [
            {
            "Chapter 1": {
                "Name": "Introduction",
                "Sections": [
                {
                    "Section 1.1": "Background Information",
                    "Section 1.2": "Research Question"
                }
                ]
            }
            },
            {
            "Chapter 2": {
                "Name": "Literature Review",
                "Sections": [
                {
                    "Section 2.1": "Relevant Studies",
                    "Section 2.2": "Theoretical Framework"
                }
                ]
            }
            }
        ]
        }"""
        
        # Create prompt for AI
        prompt = f"""You are an expert in college admissions. You have 100 podcast episodes where you discussed all nuances of college admissions process.
        Identify between 9 and 12 major chapters for the book based on the following text that contains podcast summaries and return the result in JSON format:

        {content}"""
        
        # Generate TOC using AI
        logger.info("Generating top level table of contents...")
        response = model.generate_content(prompt)
        
        if response:
            # Create output filename by replacing extension with _toc_top.txt
            base = os.path.splitext(filename)[0]
            output_file = f"{base}_toc_top.txt"
            
            # Save the AI-generated TOC
            with open(output_file, 'w', encoding='utf-8') as f:
                #f.write("COLLEGE ADMISSIONS GUIDE\n")
                #f.write("=======================\n\n")
                #f.write("TABLE OF CONTENTS\n\n")
                f.write(response.text)
            
            logger.info(f"Top level table of contents saved to: {output_file}")
            return True
        else:
            logger.error("No response received from Gemini API")
            return False
            
    except Exception as e:
        logger.error(f"Error creating top level table of contents: {str(e)}")
        return False

def main():
    # Set up argument parser
    parser = argparse.ArgumentParser(description='Process transcription files and generate table of contents')
    parser.add_argument('--dir', metavar='DIRECTORY', type=str,
                      help='Path to the directory containing transcription files')
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument('--sum', action='store_true',
                      help='Summarize text files in the directory')
    group.add_argument('--toc', action='store_true',
                      help='Create table of contents from existing summaries')
    group.add_argument('--toc-top', metavar='FILENAME',
                      help='Create AI-generated table of contents from the specified file')
    
    # Parse arguments
    args = parser.parse_args()
    
    # Validate arguments
    if (args.sum or args.toc) and not args.dir:
        parser.error("--dir is required when using --sum or --toc")
    
    if args.sum:
        # Process directory and create summaries
        total_input, total_output = process_directory(args.dir)
        return 0 if (total_input > 0 and total_output > 0) else 1
    elif args.toc:
        # Create table of contents
        success = create_toc(args.dir)
        return 0 if success else 1
    else:  # args.toc_top
        # Create AI-generated table of contents
        success = toc_chapters(args.toc_top)
        return 0 if success else 1

if __name__ == "__main__":
    exit(main()) 