import PyPDF2
import os
import re
from typing import List, Dict, Any
from config import Config
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class DocumentProcessor:
    def __init__(self):
        self.chunk_size = Config.CHUNK_SIZE
        self.chunk_overlap = Config.CHUNK_OVERLAP
    
    def clean_text(self, text: str) -> str:
        """Clean text by removing problematic characters and normalizing whitespace"""
        if not text:
            return ""
        
        # Remove NUL characters and other control characters
        text = text.replace('\x00', '')
        text = re.sub(r'[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]', '', text)
        
        # Normalize whitespace
        text = re.sub(r'\s+', ' ', text)
        text = text.strip()
        
        # Remove any remaining problematic characters
        text = text.encode('utf-8', errors='ignore').decode('utf-8')
        
        return text
    
    def extract_text_from_pdf(self, pdf_path: str) -> str:
        """Extract text content from a PDF file"""
        try:
            with open(pdf_path, 'rb') as file:
                pdf_reader = PyPDF2.PdfReader(file)
                text = ""
                
                for page_num, page in enumerate(pdf_reader.pages):
                    page_text = page.extract_text()
                    if page_text:
                        # Clean the page text
                        cleaned_text = self.clean_text(page_text)
                        if cleaned_text:
                            text += f"\n--- Page {page_num + 1} ---\n{cleaned_text}\n"
                
                # Final cleaning of the entire text
                text = self.clean_text(text)
                
                logger.info(f"Successfully extracted and cleaned text from {pdf_path}")
                return text
                
        except Exception as e:
            logger.error(f"Error extracting text from {pdf_path}: {e}")
            raise
    
    def chunk_text(self, text: str) -> List[str]:
        """Split text into overlapping chunks"""
        if len(text) <= self.chunk_size:
            return [text]
        
        chunks = []
        start = 0
        
        while start < len(text):
            end = start + self.chunk_size
            
            # If this isn't the last chunk, try to break at a sentence boundary
            if end < len(text):
                # Look for sentence endings within the last 100 characters
                search_start = max(start + self.chunk_size - 100, start)
                sentence_endings = ['.', '!', '?', '\n\n']
                
                for ending in sentence_endings:
                    pos = text.rfind(ending, search_start, end)
                    if pos != -1:
                        end = pos + 1
                        break
            
            chunk = text[start:end].strip()
            if chunk:
                # Clean each chunk before adding
                cleaned_chunk = self.clean_text(chunk)
                if cleaned_chunk:
                    chunks.append(cleaned_chunk)
            
            # Move start position, accounting for overlap
            start = end - self.chunk_overlap
            if start >= len(text):
                break
        
        logger.info(f"Text chunked into {len(chunks)} chunks")
        return chunks
    
    def process_pdf_document(self, pdf_path: str) -> List[Dict[str, Any]]:
        """Process a PDF document and return chunked content with metadata"""
        try:
            # Extract text from PDF
            text = self.extract_text_from_pdf(pdf_path)
            
            # Chunk the text
            chunks = self.chunk_text(text)
            
            # Prepare chunks with metadata
            processed_chunks = []
            for i, chunk in enumerate(chunks):
                chunk_data = {
                    'content': chunk,
                    'chunk_index': i,
                    'filename': os.path.basename(pdf_path),
                    'file_path': pdf_path,
                    'chunk_size': len(chunk),
                    'total_chunks': len(chunks)
                }
                processed_chunks.append(chunk_data)
            
            logger.info(f"Processed {pdf_path} into {len(processed_chunks)} chunks")
            return processed_chunks
            
        except Exception as e:
            logger.error(f"Error processing document {pdf_path}: {e}")
            raise
    
    def process_pdf_directory(self, directory_path: str) -> List[Dict[str, Any]]:
        """Process all PDF files in a directory"""
        all_chunks = []
        
        try:
            for filename in os.listdir(directory_path):
                if filename.lower().endswith('.pdf'):
                    pdf_path = os.path.join(directory_path, filename)
                    chunks = self.process_pdf_document(pdf_path)
                    all_chunks.extend(chunks)
            
            logger.info(f"Processed {len(all_chunks)} total chunks from directory {directory_path}")
            return all_chunks
            
        except Exception as e:
            logger.error(f"Error processing directory {directory_path}: {e}")
            raise
