import argparse
import os
import logging
import google.generativeai as genai
from dotenv import load_dotenv

class PodcastSummarizer:
    def __init__(self):
        """Initialize the PodcastSummarizer with logging and API configuration"""
        self.logger = self.setup_logging()
        self.logger.info("Initializing PodcastSummarizer...")
        
        # Load environment variables and configure API
        load_dotenv()
        self.api_key = os.getenv('GOOGLE_API_KEY')
        if not self.api_key:
            self.logger.error("GOOGLE_API_KEY not found in environment variables")
            raise ValueError("GOOGLE_API_KEY not found in environment variables")
            
        genai.configure(api_key=self.api_key)
        self.model = genai.GenerativeModel('gemini-2.0-flash')
    
    def setup_logging(self):
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
                logging.FileHandler('logs/summarize_podcast.log'),
                logging.StreamHandler()
            ]
        )
        
        return logging.getLogger(__name__)

    def read_file(self, filepath):
        """
        Read the contents of a file
        
        Args:
            filepath: Path to the file to read
            
        Returns:
            str: Contents of the file, or None if there's an error
        """
        try:
            # Check if file exists
            if not os.path.exists(filepath):
                self.logger.error(f"File not found: {filepath}")
                return None
                
            # Read file contents
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
                
            self.logger.info(f"Successfully read {len(content)} characters from {filepath}")
            return content
            
        except Exception as e:
            self.logger.error(f"Error reading file: {str(e)}")
            return None

    def count_tokens(self, text):
        """
        Count the number of tokens in the text using Gemini's tokenizer
        """
        try:
            response = self.model.count_tokens(text)
            return response.total_tokens
        except Exception as e:
            self.logger.error(f"Error counting tokens: {str(e)}")
            return None

    def summarize_text(self, text):
        """
        Summarize text using Google's Gemini API
        
        Args:
            text: Text to summarize
            
        Returns:
            tuple: (summary text, input tokens, output tokens) or (None, None, None) if error
        """
        try:
            # Create the prompt
            prompt = f"""You are an expert in college admissions. You have 100 podcast episodes where you discussed all nuances of college admissions process.
            Now you want to make a book from all podcasts. The first goal is to make a table of contents for future book.
            The following text is one of the podcasts. Summarize it. 
            Do not summarize anything not related to college admissions topic. 
            Do not summarize news. Do not summarize college admissions news. 
            Do not summarize interviews. Skip Mark's recommended resources.
            Focus on making it easier later to build a table of contents.

            {text}
            """
            
            # Count input tokens
            input_tokens = self.count_tokens(prompt)
            self.logger.info(f"Input tokens: {input_tokens}")
            
            self.logger.info("Generating summary using Gemini gemini-2.0-flash API...")
            response = self.model.generate_content(prompt)
            
            if response:
                # Count output tokens
                output_tokens = self.count_tokens(response.text)
                self.logger.info(f"Output tokens: {output_tokens}")

                token_file = os.path.join(os.path.curdir, "tokens.txt")
                with open(token_file, 'a', encoding='utf-8') as f:
                    f.write(f"{input_tokens}, {output_tokens}\n")
                
                # Calculate total tokens
                total_tokens = input_tokens + output_tokens
                self.logger.info(f"Total tokens used: {total_tokens}")
                
                self.logger.info("Summary generated successfully")
                return response.text, input_tokens, output_tokens
            else:
                self.logger.error("No response received from Gemini API")
                return None, None, None
                
        except Exception as e:
            self.logger.error(f"Error generating summary: {str(e)}")
            return None, None, None

    def process_file(self, filepath):
        """
        Process a single file: read, summarize, and save summary
        
        Args:
            filepath: Path to the file to process
            
        Returns:
            tuple: (success, input_tokens, output_tokens) where:
                - success: Boolean indicating if processing was successful
                - input_tokens: Number of input tokens used
                - output_tokens: Number of output tokens used
        """
        try:
            content = self.read_file(filepath)
            if content:
                summary, input_tokens, output_tokens = self.summarize_text(content)
                if summary:
                    # Create output filename by replacing .txt with .sum
                    base = os.path.splitext(filepath)[0]
                    output_file = f"{base}.sum"
                    
                    # Save summary to file
                    try:
                        with open(output_file, 'w', encoding='utf-8') as f:
                            f.write(summary)
                        self.logger.info(f"Summary saved to: {output_file}")
                        return True, input_tokens, output_tokens
                    except Exception as e:
                        self.logger.error(f"Error saving summary: {str(e)}")
            
            return False, None, None
            
        except Exception as e:
            self.logger.error(f"Error in process_file: {str(e)}")
            return False, None, None

def main():
    # Set up argument parser
    parser = argparse.ArgumentParser(description='Podcast transcription summarizer tool')
    parser.add_argument('--file', metavar='FILEPATH', type=str, required=True,
                      help='Path to the transcription file to summarize')
    
    # Parse arguments
    args = parser.parse_args()
    
    try:
        # Create summarizer instance and process file
        summarizer = PodcastSummarizer()
        success, input_tokens, output_tokens = summarizer.process_file(args.file)
        
        if success:
            summarizer.logger.info(f"Final token usage - Input: {input_tokens}, Output: {output_tokens}, Total: {input_tokens + output_tokens}")
        
    except Exception as e:
        print(f"Error: {str(e)}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main()) 