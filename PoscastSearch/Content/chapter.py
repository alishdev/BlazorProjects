import argparse
import os
import re
import logging
import google.generativeai as genai
from dotenv import load_dotenv

def setup_logging():
    """Set up logging configuration"""
    os.makedirs('logs', exist_ok=True)
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(levelname)s - %(message)s',
        handlers=[
            logging.FileHandler('logs/chapter.log'),
            logging.StreamHandler()
        ]
    )
    return logging.getLogger(__name__)

def clean_filename(text):
    """Remove special characters and spaces from filename"""
    # Replace spaces and special characters with underscore
    cleaned = re.sub(r'[^a-zA-Z0-9]', '_', text)
    # Remove multiple consecutive underscores
    cleaned = re.sub(r'_+', '_', cleaned)
    # Remove leading/trailing underscores
    cleaned = cleaned.strip('_')
    return cleaned.lower()

def create_chapter(source_file, chapter_name, logger):
    try:
        # Configure Gemini
        load_dotenv()
        api_key = os.getenv('GOOGLE_API_KEY')
        if not api_key:
            logger.error("GOOGLE_API_KEY not found in environment variables")
            return False
            
        genai.configure(api_key=api_key)
        model = genai.GenerativeModel('gemini-1.5-pro')

        # Read source file
        with open(source_file, 'r', encoding='utf-8') as infile:
            content = infile.read()
            
        # Create prompt and get Gemini response
        prompt = f"""You are an expert in college admissions. You want to make a book about college admissions process.
        The name of the chapter is {chapter_name}. 
        Use only the following text to create the chapter in html format. 
        The length of the chapter should be between 2000 and 3000 words. Do not include chapter number.
        Write the chapter in a way that is easy to understand for a high school student.

        {content}
        """
        
        response = model.generate_content(prompt)
        
        # Clean the response text
        text = response.text
        lines = text.split('\n')
        
        # Remove first line if it contains ```html
        if lines and lines[0].strip().startswith('```html'):
            lines = lines[1:]
            
        # Remove last line if it contains ```
        if lines and lines[-1].strip() == '```':
            lines = lines[:-1]
            
        cleaned_text = '\n'.join(lines)
            
        # Create chapter file with cleaned response
        chapter_file = f"{clean_filename(chapter_name)}.html"
        with open(chapter_file, 'w', encoding='utf-8') as outfile:
            outfile.write(cleaned_text)
            
        logger.info(f"Created chapter file: {chapter_file}")
        return True
    except Exception as e:
        logger.error(f"Error creating chapter: {str(e)}")
        return False

def process_toc_file(toc_file, source_file, logger):
    """Process each chapter from TOC file"""
    try:
        with open(toc_file, 'r', encoding='utf-8') as f:
            chapters = f.readlines()
        
        success = True
        for chapter in chapters:
            chapter = chapter.strip()
            if chapter:  # Skip empty lines
                logger.info(f"Processing chapter: {chapter}")
                if not create_chapter(source_file, chapter, logger):
                    success = False
                    logger.error(f"Failed to process chapter: {chapter}")
        
        return success
    except Exception as e:
        logger.error(f"Error processing TOC file: {str(e)}")
        return False

def main():
    logger = setup_logging()
    
    parser = argparse.ArgumentParser(description='Create chapter file from source file')
    parser.add_argument('--file', required=True, help='Source file path')
    parser.add_argument('--chapter', help='Chapter name')
    parser.add_argument('--toc', help='Path to TOC file containing chapter names')
    
    args = parser.parse_args()
    
    if args.toc:
        success = process_toc_file(args.toc, args.file, logger)
    elif args.chapter:
        success = create_chapter(args.file, args.chapter, logger)
    else:
        logger.error("Either --chapter or --toc argument is required")
        return 1
    
    return 0 if success else 1

if __name__ == "__main__":
    exit(main()) 