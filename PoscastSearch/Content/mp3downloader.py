import requests
import os
import logging
import traceback
from urllib.parse import urlparse, unquote

class MP3Downloader:
    def __init__(self):
        # Set up logging
        logging.basicConfig(level=logging.DEBUG)
        self.logger = logging.getLogger(__name__)
        
        # Set up headers for requests
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        }

    def download(self, url: str, output_dir: str = "downloads", filename: str = None) -> str:
        """
        Download an MP3 file from a URL
        
        Args:
            url (str): Direct URL to the MP3 file
            output_dir (str): Directory to save the downloaded file
            filename (str): Optional custom filename (without extension)
        
        Returns:
            str: Path to the downloaded file or None if download fails
        """
        try:
            self.logger.info(f"Starting download from URL: {url}")
            
            # Create output directory if it doesn't exist
            os.makedirs(output_dir, exist_ok=True)
            self.logger.debug(f"Output directory verified: {output_dir}")
            
            # Generate filename if not provided
            if not filename:
                # Extract filename from URL
                parsed_url = urlparse(url)
                url_filename = os.path.basename(unquote(parsed_url.path))
                # Use URL filename or default if empty
                filename = url_filename if url_filename else "audio"
            
            # Ensure .mp3 extension
            if not filename.lower().endswith('.mp3'):
                filename += '.mp3'
                
            # Clean filename of invalid characters
            filename = "".join([c for c in filename if c.isalnum() or c in (' ', '-', '_', '.')]).rstrip()
            output_path = os.path.join(output_dir, filename)
            self.logger.info(f"Output path: {output_path}")
            
            # Start download with streaming
            self.logger.info("Initiating file download")
            response = requests.get(url, headers=self.headers, stream=True)
            response.raise_for_status()
            
            # Get file size for progress tracking
            file_size = int(response.headers.get('content-length', 0))
            self.logger.info(f"File size: {file_size/1024/1024:.2f} MB")
            
            # Download and save the file
            bytes_downloaded = 0
            self.logger.info("Writing file to disk")
            with open(output_path, 'wb') as f:
                for chunk in response.iter_content(chunk_size=8192):
                    if chunk:
                        f.write(chunk)
                        bytes_downloaded += len(chunk)
                        if file_size:
                            progress = (bytes_downloaded / file_size) * 100
                            #self.logger.debug(f"Download progress: {progress:.1f}%")
            
            self.logger.info("Download completed successfully")
            return output_path
            
        except requests.exceptions.RequestException as e:
            self.logger.error(f"Network error: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return None
        except Exception as e:
            self.logger.error(f"Error downloading file: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return None

# Example usage
if __name__ == "__main__":
    downloader = MP3Downloader()
    #for episode_num in range(1, 100):
    #    print(f"YCBK_{episode_num:03d}")
    #    url = f"https://traffic.libsyn.com/secure/yourcollegeboundkid/YCBK_{episode_num:03d}.mp3"
    #    result = downloader.download(url)
    url = "https://open.spotify.com/episode/31eN0ZzLMl6zXlibchsKtz"
    result = downloader.download(url)
    if result:
        print(f"Successfully downloaded to: {result}")
    else:
        print("Download failed") 