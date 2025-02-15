import requests
from bs4 import BeautifulSoup
import urllib.parse
import os
import logging
import traceback

class PodcastDownloader:
    def __init__(self):
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        }
        # Set up logging
        logging.basicConfig(level=logging.DEBUG)
        self.logger = logging.getLogger(__name__)
    
    def download_episode(self, episode_url, output_dir="downloads"):
        """
        Download a podcast episode from Apple Podcasts
        
        Args:
            episode_url (str): URL of the podcast episode on podcasts.apple.com
            output_dir (str): Directory to save the downloaded episode
        
        Returns:
            str: Path to the downloaded file or None if download fails
        """
        try:
            self.logger.info(f"Starting download from URL: {episode_url}")
            self.logger.info(f"Output directory: {output_dir}")
            
            # Create output directory if it doesn't exist
            os.makedirs(output_dir, exist_ok=True)
            self.logger.debug(f"Output directory created/verified")
            
            # Get the episode page
            self.logger.debug(f"Fetching episode page with headers: {self.headers}")
            response = requests.get(episode_url, headers=self.headers)
            response.raise_for_status()
            self.logger.debug(f"Page fetch status code: {response.status_code}")
            
            # Parse the HTML
            self.logger.debug("Parsing HTML content")
            soup = BeautifulSoup(response.text, 'html.parser')
            
            # Find the audio source URL
            self.logger.debug("Searching for audio element")
            audio_element = soup.find('audio')
            if not audio_element:
                self.logger.error("No audio element found in page HTML")
                self.logger.debug(f"Page content: {response.text[:500]}...")  # First 500 chars of response
                raise ValueError("Could not find audio element on page")
            
            audio_url = audio_element.get('src')
            if not audio_url:
                self.logger.error("Audio element found but no src attribute")
                self.logger.debug(f"Audio element: {audio_element}")
                raise ValueError("Could not find audio source URL")
            
            self.logger.info(f"Found audio URL: {audio_url}")
            
            # Get episode title for filename
            self.logger.debug("Searching for title element")
            title_element = soup.find('h1')
            if title_element:
                episode_title = title_element.text.strip()
                self.logger.debug(f"Found title: {episode_title}")
            else:
                episode_title = "podcast_episode"
                self.logger.warning("No title found, using default filename")
            
            # Clean filename
            safe_filename = "".join([c for c in episode_title if c.isalnum() or c in (' ', '-', '_')]).rstrip()
            output_path = os.path.join(output_dir, f"{safe_filename}.mp3")
            self.logger.info(f"Output path: {output_path}")
            
            # Download the audio file
            self.logger.info("Starting audio file download")
            audio_response = requests.get(audio_url, headers=self.headers, stream=True)
            audio_response.raise_for_status()
            
            # Get file size for progress tracking
            file_size = int(audio_response.headers.get('content-length', 0))
            self.logger.info(f"File size: {file_size/1024/1024:.2f} MB")
            
            # Save the file
            bytes_downloaded = 0
            self.logger.info("Writing file to disk")
            with open(output_path, 'wb') as f:
                for chunk in audio_response.iter_content(chunk_size=8192):
                    if chunk:
                        f.write(chunk)
                        bytes_downloaded += len(chunk)
                        if file_size:
                            progress = (bytes_downloaded / file_size) * 100
                            self.logger.debug(f"Download progress: {progress:.1f}%")
            
            self.logger.info("Download completed successfully")
            return output_path
            
        except requests.exceptions.RequestException as e:
            self.logger.error(f"Network error: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return None
        except Exception as e:
            self.logger.error(f"Error downloading podcast: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return None

# Example usage
downloader = PodcastDownloader()
#episode_url = "https://podcasts.apple.com/us/podcast/understanding-how-duke-makes-admissions-decision-part/id1349060136?i=1000685582807"
#episode_url = "https://traffic.libsyn.com/secure/yourcollegeboundkid/YCBK_505.mp3"
episode_url = "https://open.spotify.com/episode/31eN0ZzLMl6zXlibchsKtz"
downloaded_file = downloader.download_episode(episode_url)

if downloaded_file:
    print(f"Successfully downloaded to: {downloaded_file}")
else:
    print("Download failed")
