import os
import re
from typing import List, Tuple

try:
    import PyPDF2
except ImportError:
    PyPDF2 = None

def deduplicate_questions(questions):
    """Deduplicate questions (case-insensitive, ignore punctuation/whitespace)."""
    # Normalize questions by removing quotes and question numbers
    normalized_questions = []
    seen_questions = set()
    unique_questions = []
    
    for question in questions:
        if not isinstance(question, str):
            continue
            
        # Remove enclosing quotes (single or double quotes)
        normalized = question.strip()
        if (normalized.startswith('"') and normalized.endswith('"')) or \
           (normalized.startswith("'") and normalized.endswith("'")):
            normalized = normalized[1:-1].strip()
        
        # Remove question numbers at the beginning (e.g., "12.", "1.", "123.")
        normalized = re.sub(r'^\d+\.\s*', '', normalized)
        
        # Convert to lowercase for case-insensitive comparison
        normalized_lower = normalized.lower()
        
        # Check if we've seen this question before (case-insensitive)
        if normalized_lower not in seen_questions:
            seen_questions.add(normalized_lower)
            unique_questions.append(normalized)  # Keep original case of first occurrence
    
    return unique_questions

def load_documents(source_dir: str) -> List[Tuple[str, dict]]:
    """Load all .pdf, .txt, .md files from source_dir. Returns list of (text, metadata) tuples."""
    docs = []
    for fname in os.listdir(source_dir):
        fpath = os.path.join(source_dir, fname)
        if fname.lower().endswith('.pdf') and PyPDF2:
            with open(fpath, 'rb') as f:
                reader = PyPDF2.PdfReader(f)
                text = "\n".join(page.extract_text() or '' for page in reader.pages)
                docs.append((text, {'filename': fname}))
        elif fname.lower().endswith(('.txt', '.md')):
            with open(fpath, 'r', encoding='utf-8') as f:
                text = f.read()
                docs.append((text, {'filename': fname}))
    return docs

def chunk_text(text: str, chunk_size: int = 512, chunk_overlap: int = 64) -> List[str]:
    """Chunk text into overlapping segments."""
    # Simple whitespace-based chunking
    words = re.split(r'\s+', text)
    chunks = []
    i = 0
    while i < len(words):
        chunk = " ".join(words[i:i+chunk_size])
        if chunk.strip():
            chunks.append(chunk)
        i += chunk_size - chunk_overlap
    return chunks 