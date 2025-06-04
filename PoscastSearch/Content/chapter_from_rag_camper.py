import argparse
import os
import re
import logging
import time
import google.generativeai as genai
from dotenv import load_dotenv
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from langchain_community.vectorstores import Chroma

def setup_logging():
    """Set up logging configuration"""
    os.makedirs('logs', exist_ok=True)
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(levelname)s - %(message)s',
        handlers=[
            logging.FileHandler('logs/chapter_rag.log'),
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

def get_relevant_content(db_path, query, logger):
    """Query vector database for relevant content"""
    try:
        logger.info(f"Initializing embeddings for query: '{query}'")
        # Initialize Google embeddings
        embeddings = GoogleGenerativeAIEmbeddings(model="models/embedding-001")
        
        logger.info(f"Loading vector store from: {db_path}")
        # Load the vector store
        db = Chroma(
            collection_name="pod_collection",
            embedding_function=embeddings,
            persist_directory=db_path
        )
        
        logger.info(f"Total documents in database: {db._collection.count()}")
        
        # Query the database
        logger.info(f"Querying database for top {15} relevant chunks...")
        results = db.similarity_search(query, k=15)
        
        # Check if results were returned
        if not results:
            error_msg = f"No relevant content found in database for query: '{query}'"
            logger.error(error_msg)
            raise ValueError(error_msg)
            
        logger.info(f"Retrieved {len(results)} chunks from database")
        
        # Combine all chunks into one text
        content = "\n\n".join([doc.page_content for doc in results])
        
        # Add source information
        sources = set(doc.metadata['source'] for doc in results)
        logger.info(f"Found {len(sources)} unique sources:")
        for source in sorted(sources):
            logger.info(f"  - {source}")
            
        content += "\n\nSources:\n" + "\n".join(sources)
        
        logger.info(f"Total content length: {len(content)} characters")
        return content
    except Exception as e:
        logger.error(f"Error querying vector database: {str(e)}")
        return None

def create_chapter(db_path, chapter_name, logger):
    try:
        # Configure Gemini
        load_dotenv()
        api_key = os.getenv('GOOGLE_API_KEY')
        if not api_key:
            logger.error("GOOGLE_API_KEY not found in environment variables")
            return False
            
        genai.configure(api_key=api_key)
        model = genai.GenerativeModel('gemini-2.0-flash')

        # Get relevant content from vector database
        content = get_relevant_content(db_path, chapter_name, logger)
        if not content:
            return False
            
        # Extract sources before they're passed to the LLM
        sources = content.split("Sources:\n")[-1].split("\n")[:3]
            
        # Create prompt and get Gemini response
        prompt = f"""You are a seasoned marketing and communications consultant specializing in the summer camp industry and you are writing the ultimate guide to effectively marketing your summer camp to families
        In this comprehensive guide, you'll teach a simple-to-follow system to revolutionize how you attract and engage with families, ensuring your camp's success and growth.
        The book covers: marketing plans tailored for summer camps, communication strategies to connect with parents and campers and insights into the summer camp industry.
        Write the first chapter with the name {chapter_name}. 
        Use only the following text to create the chapter. 
        The length of the chapter should be between 2000 and 3000 words. Do not include chapter number.
        Write the chapter in a way that is easy to understand for a high school student.
        Format the output as HTML with proper headings and paragraphs.

        Content:
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

        # Add sources section at the bottom
        sources_html = """
        <hr>
        <div class="sources">
            <h3>Sources:</h3>
            <ul>
        """
        for source in sources:
            sources_html += f"    <li>{source}</li>\n"
        sources_html += """
            </ul>
        </div>
        """
        
        final_html = cleaned_text + sources_html
            
        # Create chapter file with cleaned response
        chapter_file = f"{clean_filename(chapter_name)}.html"
        with open(chapter_file, 'w', encoding='utf-8') as outfile:
            outfile.write(final_html)
            
        logger.info(f"Created chapter file: {chapter_file}")
        logger.info(f"Added {len(sources)} sources to the chapter")
        return True
    except Exception as e:
        logger.error(f"Error creating chapter: {str(e)}")
        return False

def process_toc_file(toc_file, db_path, logger):
    """Process each chapter from TOC file"""
    try:
        with open(toc_file, 'r', encoding='utf-8') as f:
            chapters = f.readlines()
        
        success = True
        for chapter in chapters:
            chapter = chapter.strip()
            if chapter:  # Skip empty lines
                logger.info(f"Processing chapter: {chapter}")
                if not create_chapter(db_path, chapter, logger):
                    success = False
                    logger.error(f"Failed to process chapter: {chapter}")
                time.sleep(60)
        
        return success
    except Exception as e:
        logger.error(f"Error processing TOC file: {str(e)}")
        return False

def main():
    logger = setup_logging()
    
    parser = argparse.ArgumentParser(description='Create chapter file from vector database using RAG')
    parser.add_argument('--dbpath', required=True, help='Path to the vector database')
    parser.add_argument('--chapter', help='Chapter name')
    parser.add_argument('--toc', help='Path to TOC file containing chapter names')
    
    args = parser.parse_args()
    
    if args.toc:
        success = process_toc_file(args.toc, args.dbpath, logger)
    elif args.chapter:
        success = create_chapter(args.dbpath, args.chapter, logger)
    else:
        logger.error("Either --chapter or --toc argument is required")
        return 1
    
    return 0 if success else 1

if __name__ == "__main__":
    exit(main()) 