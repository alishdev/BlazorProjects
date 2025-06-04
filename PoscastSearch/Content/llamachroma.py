import argparse
import os
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.embeddings import OllamaEmbeddings
from langchain_community.vectorstores import Chroma
from dotenv import load_dotenv
import logging
from langchain.schema import Document
import time  # Add at the top with other imports

DB = None

def initialize_embeddings(db_dir):
    """Initialize embeddings"""
    global DB  # Add global declaration
    # Initialize Ollama embeddings
    embeddings = OllamaEmbeddings(
        model="llama3.2",  # or any other model you have in Ollama
        base_url="http://localhost:11434"  # default Ollama API endpoint
    )
    DB = Chroma(
        collection_name="pod_collection",
        embedding_function=embeddings,
        persist_directory=db_dir
    )

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
            chunk_size=1000,
            chunk_overlap=200,
            length_function=len,
            add_start_index=True,
        )

        # Split text into chunks
        chunks = text_splitter.split_text(content)
        filename = os.path.basename(filepath)
        
        # Convert chunks to Document objects
        documents = [
            Document(
                page_content=chunk,
                metadata={"source": filename}
            ) for chunk in chunks
        ]
        
        DB.add_documents(documents)
        logger.info(f"Added {len(documents)} chunks from {filename}")
        return True

    except Exception as e:
        logger.error(f"Error processing file: {str(e)}")
        return False

def main():
    # Set up logging
    logger = setup_logging()
    
    # Load environment variables
    load_dotenv()

    # Parse arguments
    parser = argparse.ArgumentParser(description='Process text files into Chroma vector database')
    parser.add_argument('--folder', metavar='FOLDERPATH', type=str, required=True,
                      help='Path to the folder containing files to process')
    parser.add_argument('--dbname', metavar='DBPATH', type=str, required=True,
                      help='Directory path to save the database')

    args = parser.parse_args()

    # Create database directory if it doesn't exist
    os.makedirs(args.dbname, exist_ok=True)

    # Initialize embeddings with specified database directory
    initialize_embeddings(args.dbname)

    # Process all files in the folder
    folder_path = args.folder
    if not os.path.isdir(folder_path):
        logger.error(f"'{folder_path}' is not a valid directory")
        return 1

    success = True
    for filename in os.listdir(folder_path):
        file_path = os.path.join(folder_path, filename)
        if os.path.isfile(file_path):
            logger.info(f"Processing file: {filename}")
            if not process_file(file_path, logger):
                success = False
            #time.sleep(30)  # Add 30 second delay between files

    return 0 if success else 1

if __name__ == "__main__":
    exit(main()) 