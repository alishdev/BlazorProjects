import os
from typing import Dict, Optional
from dotenv import load_dotenv
import openai
import google.generativeai as genai
import anthropic

# Load environment variables from .env file
load_dotenv()

# Debug: Check what anthropic module we're importing
print(f"[DEBUG] Anthropic module imported: {anthropic}")
print(f"[DEBUG] Anthropic module version: {getattr(anthropic, '__version__', 'unknown')}")
print(f"[DEBUG] Available attributes in anthropic: {dir(anthropic)}")

class AIAgents:
    """
    A class to handle AI agent interactions with different LLMs.
    """
    
    def __init__(self):
        """
        Initialize the AIAgents class and load API keys for all supported LLMs.
        """
        self.api_keys: Dict[str, Optional[str]] = {
            'openai': os.getenv('OPENAI_API_KEY'),
            'gemini': os.getenv('GEMINI_API_KEY'),
            'anthropic': os.getenv('ANTHROPIC_API_KEY'),
            'cohere': os.getenv('COHERE_API_KEY'),
            'huggingface': os.getenv('HUGGINGFACE_API_KEY'),
            'azure_openai': os.getenv('AZURE_OPENAI_API_KEY'),
            'claude': os.getenv('CLAUDE_API_KEY'),
            'mistral': os.getenv('MISTRAL_API_KEY'),
            'perplexity': os.getenv('PERPLEXITY_API_KEY'),
            'groq': os.getenv('GROQ_API_KEY')
        }
        
        # Log which API keys are available (without exposing the actual keys)
        available_apis = [name for name, key in self.api_keys.items() if key is not None]
        print(f"Available LLM APIs: {available_apis}")
    
    def answer_open_ai(self, model: str, prompt: str) -> str:
        """
        Make an API call to OpenAI with the specified model and prompt.
        
        Args:
            model (str): The OpenAI model to use (e.g., 'gpt-4', 'gpt-3.5-turbo')
            prompt (str): The prompt to send to the model
            
        Returns:
            str: The response from OpenAI
            
        Raises:
            ValueError: If no OpenAI API key is configured
            Exception: If the API call fails
        """
        # Check if OpenAI API key is available
        if not self.api_keys['openai']:
            raise ValueError("No OpenAI API key configured. Please set OPENAI_API_KEY environment variable.")
        
        try:
            # Configure OpenAI client with API key
            client = openai.OpenAI(api_key=self.api_keys['openai'])
            
            # Make the API call
            response = client.chat.completions.create(
                model=model,
                messages=[
                    {"role": "user", "content": prompt}
                ],
                max_tokens=1000,  # Adjust as needed
                temperature=0.7   # Adjust as needed
            )
            
            # Extract and return the response
            return response.choices[0].message.content
            
        except openai.AuthenticationError:
            raise Exception("Invalid OpenAI API key. Please check your OPENAI_API_KEY.")
        except openai.RateLimitError:
            raise Exception("OpenAI API rate limit exceeded. Please try again later.")
        except openai.APIError as e:
            raise Exception(f"OpenAI API error: {str(e)}")
        except Exception as e:
            raise Exception(f"Unexpected error calling OpenAI API: {str(e)}")

    def answer_gemini(self, model: str, prompt: str) -> str:
        """
        Make an API call to Google Gemini with the specified model and prompt.
        
        Args:
            model (str): The Gemini model to use (e.g., 'gemini-pro', 'gemini-pro-vision')
            prompt (str): The prompt to send to the model
            
        Returns:
            str: The response from Gemini
            
        Raises:
            ValueError: If no Gemini API key is configured
            Exception: If the API call fails
        """
        # Check if Gemini API key is available
        if not self.api_keys['gemini']:
            raise ValueError("No Gemini API key configured. Please set GEMINI_API_KEY environment variable.")
        
        try:
            # Configure Gemini with API key
            genai.configure(api_key=self.api_keys['gemini'])
            
            # Create the model instance
            gemini_model = genai.GenerativeModel(model)
            
            # Generate content
            response = gemini_model.generate_content(prompt)
            
            # Extract and return the response
            return response.text
            
        except Exception as e:
            if "API_KEY_INVALID" in str(e):
                raise Exception("Invalid Gemini API key. Please check your GEMINI_API_KEY.")
            elif "QUOTA_EXCEEDED" in str(e):
                raise Exception("Gemini API quota exceeded. Please try again later.")
            elif "MODEL_NOT_FOUND" in str(e):
                raise Exception(f"Gemini model '{model}' not found. Please check the model name.")
            else:
                raise Exception(f"Unexpected error calling Gemini API: {str(e)}")

    def answer_anthropic(self, model: str, prompt: str) -> str:
        """
        Make an API call to Anthropic Claude with the specified model and prompt.
        
        Args:
            model (str): The Claude model to use (e.g., 'claude-3-sonnet-20240229', 'claude-3-opus-20240229')
            prompt (str): The prompt to send to the model
            
        Returns:
            str: The response from Claude
            
        Raises:
            ValueError: If no Anthropic API key is configured
            Exception: If the API call fails
        """
        print(f"[DEBUG] answer_anthropic called with model: {model}, prompt length: {len(prompt)}")
        
        # Check if Anthropic API key is available
        if not self.api_keys['anthropic']:
            print("[DEBUG] No Anthropic API key found")
            raise ValueError("No Anthropic API key configured. Please set ANTHROPIC_API_KEY environment variable.")
        
        print(f"[DEBUG] Anthropic API key found: {self.api_keys['anthropic'][:10]}...")
        
        try:
            print("[DEBUG] Creating Anthropic client...")
            
            # Try different client initialization approaches
            try:
                # Method 1: Direct initialization
                anthropic_api_key = self.api_keys['anthropic']
                print("[DEBUG] Trying direct client initialization..." + anthropic_api_key)
                client = anthropic.Client(api_key=self.api_keys['anthropic'])
                print("[DEBUG] Direct client initialization successful")
            except TypeError as e:
                print(f"[DEBUG] Direct initialization failed: {e}")
                try:
                    # Method 2: Create client without parameters, then set API key
                    print("[DEBUG] Trying client creation without parameters...")
                    client = anthropic.Client()
                    client.api_key = self.api_keys['anthropic']
                    print("[DEBUG] Client creation without parameters successful")
                except Exception as e2:
                    print(f"[DEBUG] Client creation without parameters failed: {e2}")
                    # Method 3: Try using Anthropic class instead of Client
                    print("[DEBUG] Trying Anthropic class instead of Client...")
                    client = anthropic.Anthropic(api_key=self.api_keys['anthropic'])
                    print("[DEBUG] Anthropic class initialization successful")
            
            print("[DEBUG] Anthropic client created successfully")
            
            print(f"[DEBUG] Making API call to Anthropic with model: {model}")
            # Make the API call using the older API format
            response = client.messages.create(
                model=model,
                max_tokens=1000,  # Adjust as needed
                messages=[
                    {
                        "role": "user",
                        "content": prompt
                    }
                ]
            )
            print("[DEBUG] Anthropic API call completed successfully")
            
            # Extract and return the response
            result = response.content[0].text
            print(f"[DEBUG] Anthropic response length: {len(result)}")
            return result
            
        except anthropic.AuthenticationError as e:
            print(f"[DEBUG] Anthropic AuthenticationError: {e}")
            raise Exception("Invalid Anthropic API key. Please check your ANTHROPIC_API_KEY.")
        except anthropic.RateLimitError as e:
            print(f"[DEBUG] Anthropic RateLimitError: {e}")
            raise Exception("Anthropic API rate limit exceeded. Please try again later.")
        except anthropic.APIError as e:
            print(f"[DEBUG] Anthropic APIError: {e}")
            raise Exception(f"Anthropic API error: {str(e)}")
        except Exception as e:
            print(f"[DEBUG] Unexpected error in Anthropic call: {e}")
            print(f"[DEBUG] Error type: {type(e)}")
            print(f"[DEBUG] Error details: {str(e)}")
            raise Exception(f"Unexpected error calling Anthropic API: {str(e)}")

    def answer(self, llm_name: str, prompt: str, model: str = None) -> str:
        """
        Generate an answer using the specified LLM.
        
        Args:
            llm_name (str): The name of the LLM to use
            prompt (str): The prompt to send to the LLM
            model (str, optional): The specific model to use (e.g., 'gpt-4', 'gpt-3.5-turbo', 'claude-3-sonnet')
            
        Returns:
            str: The response from the LLM
        """
        # Check if API key is available for the requested LLM
        llm_key = llm_name.lower()
        if llm_key not in self.api_keys:
            return f"Error: Unsupported LLM '{llm_name}'. Supported LLMs: {list(self.api_keys.keys())}"
        
        if self.api_keys[llm_key] is None:
            return f"Error: No API key configured for '{llm_name}'. Please set the corresponding environment variable."
        
        # Route to specific LLM implementation
        if llm_key == 'openai':
            if not model:
                model = 'gpt-3.5-turbo'  # Default model
            try:
                return self.answer_open_ai(model, prompt)
            except Exception as e:
                return f"Error calling OpenAI API: {str(e)}"
        
        elif llm_key == 'gemini':
            if not model:
                model = 'gemini-pro'  # Default model
            try:
                return self.answer_gemini(model, prompt)
            except Exception as e:
                return f"Error calling Gemini API: {str(e)}"
        
        elif llm_key == 'anthropic':
            if not model:
                model = 'claude-3-sonnet-20240229'  # Default model
            try:
                return self.answer_anthropic(model, prompt)
            except Exception as e:
                return f"Error calling Anthropic API: {str(e)}"
        
        # This is a placeholder implementation for other LLMs
        # In a real application, you would integrate with actual LLM APIs here
        model_info = f" (model: {model})" if model else ""
        response = f"{llm_name} response to: {prompt} (API key available){model_info}"
        return response
    
    def get_available_llms(self) -> list:
        """
        Get a list of LLMs that have API keys configured.
        
        Returns:
            list: List of LLM names that have API keys available
        """
        return [name for name, key in self.api_keys.items() if key is not None] 