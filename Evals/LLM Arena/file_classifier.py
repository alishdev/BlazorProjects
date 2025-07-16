import os
import json
import openai
from pathlib import Path
import PyPDF2
import yaml
from typing import Dict, List, Tuple
import time

class FileClassifier:
    def __init__(self, config_path="config.yaml"):
        """Initialize the file classifier with configuration."""
        self.config = self.load_config(config_path)
        self.setup_openai()
        self.hierarchy = {}
        
    def load_config(self, config_path):
        """Load configuration from YAML file."""
        if not os.path.exists(config_path):
            raise FileNotFoundError(f"Config file not found: {config_path}")
        with open(config_path, 'r') as f:
            return yaml.safe_load(f)
    
    def setup_openai(self):
        """Setup OpenAI client with API key from config."""
        # Find OpenAI provider in config
        openai_provider = None
        for provider in self.config.get('llm_providers', []):
            if 'openai' in provider.get('name', '').lower():
                openai_provider = provider
                break
        
        if not openai_provider:
            raise ValueError("No OpenAI provider found in config")
        
        api_key = openai_provider.get('api_key')
        if not api_key:
            raise ValueError("OpenAI API key not found in config")
        
        openai.api_key = api_key
    
    def extract_text_from_pdf(self, pdf_path: str) -> str:
        """Extract text content from a PDF file."""
        try:
            with open(pdf_path, 'rb') as file:
                pdf_reader = PyPDF2.PdfReader(file)
                text = ""
                for page in pdf_reader.pages:
                    text += page.extract_text() + " "
                return text.strip()
        except Exception as e:
            print(f"Error reading PDF {pdf_path}: {e}")
            return ""
    
    def classify_file_with_openai(self, filename: str, content: str) -> Dict:
        """Use OpenAI to classify a file into a 3-level hierarchy."""
        prompt = f"""
        Analyze this file and classify it into a 3-level hierarchy structure.
        
        File name: {filename}
        Content preview: {content[:1000]}...
        
        Please classify this file into a hierarchical structure with exactly 3 levels:
        - Level 1: Main category (e.g., "Programs", "Locations", "Forms", "Information")
        - Level 2: Sub-category (e.g., "Camps", "Head Start", "Registration", "General Info")
        - Level 3: Specific type or location (e.g., "Summer Camps", "Baltimore City", "Application Forms", "FAQs")
        
        Return your response as a JSON object with exactly these fields:
        {{
            "level1": "Main category name",
            "level2": "Sub-category name", 
            "level3": "Specific type name"
        }}
        
        Make sure the classification is logical and consistent. Each level should be a meaningful category.
        """
        
        try:
            client = openai.OpenAI(api_key=openai.api_key)
            response = client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[
                    {"role": "system", "content": "You are a file classification expert. Always respond with valid JSON."},
                    {"role": "user", "content": prompt}
                ],
                temperature=0.3,
                max_tokens=200
            )
            
            # Extract JSON from response
            response_text = response.choices[0].message.content.strip()
            
            # Try to parse JSON from the response
            if response_text.startswith('{') and response_text.endswith('}'):
                return json.loads(response_text)
            else:
                # If response is not clean JSON, try to extract it
                start_idx = response_text.find('{')
                end_idx = response_text.rfind('}') + 1
                if start_idx != -1 and end_idx > start_idx:
                    json_str = response_text[start_idx:end_idx]
                    return json.loads(json_str)
                else:
                    raise ValueError("Could not extract JSON from response")
                    
        except Exception as e:
            print(f"Error classifying {filename}: {e}")
            # Return default classification
            return {
                "level1": "Uncategorized",
                "level2": "General",
                "level3": "Other"
            }
    
    def build_hierarchy(self, source_docs_path: str):
        """Build the file hierarchy by processing all PDF files."""
        source_path = Path(source_docs_path)
        
        if not source_path.exists():
            raise FileNotFoundError(f"Source documents path not found: {source_docs_path}")
        
        pdf_files = list(source_path.glob("*.pdf"))
        print(f"Found {len(pdf_files)} PDF files to classify...")
        
        for i, pdf_file in enumerate(pdf_files, 1):
            print(f"Processing {i}/{len(pdf_files)}: {pdf_file.name}")
            
            # Extract text content
            content = self.extract_text_from_pdf(str(pdf_file))
            if not content:
                print(f"Skipping {pdf_file.name} - no content extracted")
                continue
            
            # Classify with OpenAI
            classification = self.classify_file_with_openai(pdf_file.name, content)
            
            # Add to hierarchy
            level1 = classification.get('level1', 'Uncategorized')
            level2 = classification.get('level2', 'General')
            level3 = classification.get('level3', 'Other')
            
            if level1 not in self.hierarchy:
                self.hierarchy[level1] = {}
            
            if level2 not in self.hierarchy[level1]:
                self.hierarchy[level1][level2] = {}
            
            if level3 not in self.hierarchy[level1][level2]:
                self.hierarchy[level1][level2][level3] = []
            
            self.hierarchy[level1][level2][level3].append(pdf_file.name)
            
            # Add small delay to avoid rate limiting
            time.sleep(0.5)
    
    def print_hierarchy(self):
        """Print the hierarchical structure in a tree format."""
        print("\n" + "="*60)
        print("FILE HIERARCHY STRUCTURE")
        print("="*60)
        
        for level1, level1_items in self.hierarchy.items():
            print(f"\n📁 {level1}")
            for level2, level2_items in level1_items.items():
                print(f"  📂 {level2}")
                for level3, files in level2_items.items():
                    print(f"    📄 {level3}")
                    for file in files:
                        print(f"      • {file}")
    
    def save_hierarchy(self, output_path: str = "file_hierarchy.json"):
        """Save the hierarchy to a JSON file."""
        with open(output_path, 'w') as f:
            json.dump(self.hierarchy, f, indent=2)
        print(f"\nHierarchy saved to: {output_path}")

def main():
    """Main function to run the file classifier."""
    try:
        # Initialize classifier
        classifier = FileClassifier()
        
        # Get source documents path from config
        source_docs_path = classifier.config.get('source_documents_path', './data/source_docs')
        
        # Build hierarchy
        classifier.build_hierarchy(source_docs_path)
        
        # Print results
        classifier.print_hierarchy()
        
        # Save to file
        classifier.save_hierarchy()
        
    except Exception as e:
        print(f"Error: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main()) 