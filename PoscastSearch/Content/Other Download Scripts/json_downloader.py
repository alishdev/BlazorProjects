import requests
import json
import logging
import traceback
from typing import Dict, Any, Optional, List
from pathlib import Path

class JSONDownloader:
    def __init__(self):
        # Set up logging
        logging.basicConfig(level=logging.DEBUG)
        self.logger = logging.getLogger(__name__)
        
        # Set up headers for requests
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Accept': 'application/json'
        }
        
    def fetch_json(self, url: str, params: Optional[Dict] = None) -> Optional[Dict[str, Any]]:
        """
        Fetch JSON data from a URL
        
        Args:
            url (str): URL to fetch JSON from
            params (dict, optional): Query parameters to include in the request
            
        Returns:
            dict: JSON response data or None if request fails
        """
        try:
            self.logger.info(f"Fetching JSON from URL: {url}")
            if params:
                self.logger.debug(f"With parameters: {params}")
            
            response = requests.get(url, headers=self.headers, params=params)
            response.raise_for_status()  # Raise exception for bad status codes
            
            self.logger.debug(f"Response status code: {response.status_code}")
            self.logger.debug(f"Response headers: {response.headers}")
            
            return response.json()
            
        except requests.exceptions.RequestException as e:
            self.logger.error(f"Network error: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return None
        except json.JSONDecodeError as e:
            self.logger.error(f"JSON decode error: {str(e)}")
            self.logger.error(f"Response content: {response.text[:500]}...")  # First 500 chars
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return None
        except Exception as e:
            self.logger.error(f"Unexpected error: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return None
    
    def save_json(self, data: Dict, filename: str, output_dir: str = "downloads") -> Optional[str]:
        """
        Save JSON data to a file
        
        Args:
            data (dict): JSON data to save
            filename (str): Name of the file to save (with or without .json extension)
            output_dir (str): Directory to save the file in
            
        Returns:
            str: Path to saved file or None if save fails
        """
        try:
            # Create output directory if it doesn't exist
            output_path = Path(output_dir)
            output_path.mkdir(parents=True, exist_ok=True)
            
            # Ensure filename has .json extension
            if not filename.lower().endswith('.json'):
                filename += '.json'
            
            # Create full file path
            file_path = output_path / filename
            self.logger.info(f"Saving JSON to: {file_path}")
            
            # Save the file with pretty printing
            with open(file_path, 'w', encoding='utf-8') as f:
                json.dump(data, f, indent=2, ensure_ascii=False)
            
            self.logger.info("JSON file saved successfully")
            return str(file_path)
            
        except Exception as e:
            self.logger.error(f"Error saving JSON file: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return None
    
    def fetch_and_save(self, url: str, filename: str, params: Optional[Dict] = None, 
                      output_dir: str = "downloads") -> Optional[str]:
        """
        Fetch JSON from URL and save it to a file
        
        Args:
            url (str): URL to fetch JSON from
            filename (str): Name of the file to save
            params (dict, optional): Query parameters for the request
            output_dir (str): Directory to save the file in
            
        Returns:
            str: Path to saved file or None if operation fails
        """
        data = self.fetch_json(url, params)
        if data:
            return self.save_json(data, filename, output_dir)
        return None

class PodcastJSONParser:
    def __init__(self):
        # Set up logging
        logging.basicConfig(level=logging.DEBUG)
        self.logger = logging.getLogger(__name__)

    def parse_json_file(self, file_path: str) -> Optional[List[Dict]]:
        """
        Parse JSON file and extract trackName and episodeUrl for each item
        
        Args:
            file_path (str): Path to the JSON file
            
        Returns:
            list: List of dictionaries containing trackName and episodeUrl
        """
        try:
            self.logger.info(f"Reading JSON file: {file_path}")
            
            # Check if file exists
            if not Path(file_path).exists():
                self.logger.error(f"File not found: {file_path}")
                return None
            
            # Read and parse JSON file
            with open(file_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
            
            # Extract results if they exist
            results = data.get('results', [])
            if not results:
                self.logger.warning("No results found in JSON data")
                return []
            
            # Extract required fields
            episodes = []
            for idx, item in enumerate(results, 1):
                episode = {
                    'trackName': item.get('trackName', 'Unknown Title'),
                    'episodeUrl': item.get('episodeUrl', 'No URL Available')
                }
                episodes.append(episode)
                
            self.logger.info(f"Successfully parsed {len(episodes)} episodes")
            return episodes
            
        except json.JSONDecodeError as e:
            self.logger.error(f"JSON parsing error: {str(e)}")
            return None
        except Exception as e:
            self.logger.error(f"Error processing file: {str(e)}")
            return None
    
    def print_episodes(self, episodes: List[Dict]):
        """
        Print episodes in a formatted way
        
        Args:
            episodes (list): List of episode dictionaries
        """
        if not episodes:
            print("No episodes to display")
            return
            
        print("\nPodcast Episodes:")
        print("-" * 80)
        for idx, episode in enumerate(episodes, 1):
            print(f"\n{idx}. Track Name: {episode['trackName']}")
            print(f"   Episode URL: {episode['episodeUrl']}")
            print("-" * 80)

# Example usage
if __name__ == "__main__":
    downloader = JSONDownloader()
    
    # Example API URL (replace with your actual API endpoint)
    api_url = "https://itunes.apple.com/lookup?id=1349060136&country=US&media=podcast&entity=podcastEpisode&limit=600"
    
    # Optional parameters
    params = {
        "limit": 100,
        "offset": 0
    }
    
    # Method 1: Fetch and process JSON data
    json_data = downloader.fetch_json(api_url, params)
    if json_data:
        print("Data fetched successfully")
        
    # Method 2: Fetch and save directly to file
    result = downloader.fetch_and_save(
        url=api_url,
        filename="api_response",
        params=params,
        output_dir="json_data"
    )
    
    if result:
        print(f"JSON saved to: {result}")
    else:
        print("Failed to fetch or save JSON")

    parser = PodcastJSONParser()
    
    # Specify your JSON file path
    json_file = "json_data/api_response.json"
    
    # Parse the file
    episodes = parser.parse_json_file(json_file)
    
    if episodes:
        parser.print_episodes(episodes)
    else:
        print("Failed to parse episodes from JSON file") 