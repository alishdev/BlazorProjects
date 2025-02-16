import argparse
import os
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_openai import OpenAIEmbeddings
from langchain_community.vectorstores import Chroma
from dotenv import load_dotenv
import logging

def setup_logging():
    """Set up logging configuration"""
    os.makedirs('logs', exist_ok=True)
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(levelname)s - %(message)s',
        handlers=[
            logging.FileHandler('logs/chroma.log'),
            logging.StreamHandler()
        ]
    )
    return logging.getLogger(__name__)

def read_file(filepath, logger):
    """Read content from file"""
    try:
        with open(filepath, 'r', encoding='utf-8') as file:
            return file.read()
    except Exception as e:
        logger.error(f"Error reading file: {str(e)}")
        return None

def process_file(filepath, logger):
    """Process file and store in Chroma"""
    try:
        # Read the file
        content = read_file(filepath, logger)
        if not content:
            return False

        # Create text splitter
        text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=10000,
            chunk_overlap=1000,
            length_function=len,
        )

        # Split text into chunks
        chunks = text_splitter.split_text(content)
        logger.info(f"Split text into {len(chunks)} chunks")

        # Initialize OpenAI embeddings
        embeddings = OpenAIEmbeddings()

        # Create and persist Chroma database
        db = Chroma.from_texts(
            texts=chunks,
            embedding=embeddings,
            persist_directory="./chroma_db"
        )
        db.persist()
        
        logger.info(f"Successfully stored {len(chunks)} chunks in Chroma database")
        return True

    except Exception as e:
        logger.error(f"Error processing file: {str(e)}")
        return False

def main():
    # Set up logging
    logger = setup_logging()
    
    # Load environment variables
    load_dotenv()
    if not os.getenv('OPENAI_API_KEY'):
        logger.error("OPENAI_API_KEY not found in environment variables")
        return 1

    # Parse arguments
    parser = argparse.ArgumentParser(description='Process text file into Chroma vector database')
    parser.add_argument('--file', metavar='FILEPATH', type=str, required=True,
                      help='Path to the text file to process')

    args = parser.parse_args()

    # Process the file
    success = process_file(args.file, logger)
    return 0 if success else 1

if __name__ == "__main__":
    exit(main()) 