import feedparser
from datetime import datetime
from typing import List, Dict
import traceback
import logging
import ssl
import certifi

# Add this right after the imports, before the class definition
ssl._create_default_https_context = ssl._create_unverified_context  # Option 1: Less secure but quick fix
# OR
# feedparser.PREFERRED_XML_PARSERS.append('html.parser')  # Option 2: Alternative fix

class PodcastFeed:
    def __init__(self, feed_url: str):
        """
        Initialize PodcastFeed with RSS feed URL
        
        Args:
            feed_url (str): URL of the podcast RSS feed
        """
        self.feed_url = feed_url
        self.feed_data = None
        # Set up logging
        logging.basicConfig(level=logging.DEBUG)
        self.logger = logging.getLogger(__name__)
        
    def fetch_feed(self) -> bool:
        """
        Fetch and parse the RSS feed
        
        Returns:
            bool: True if successful, False otherwise
        """
        try:
            self.logger.info(f"Attempting to fetch feed from: {self.feed_url}")
            self.feed_data = feedparser.parse(self.feed_url)
            
            # Check for feedparser errors
            if hasattr(self.feed_data, 'bozo_exception'):
                self.logger.error(f"Feedparser error: {self.feed_data.bozo_exception}")
                return False
                
            # Log feed response details
            self.logger.debug(f"Feed version: {self.feed_data.version}")
            self.logger.debug(f"Feed headers: {self.feed_data.headers if hasattr(self.feed_data, 'headers') else 'No headers'}")
            self.logger.debug(f"Number of entries: {len(self.feed_data.entries)}")
            
            if len(self.feed_data.entries) > 0:
                self.logger.info("Successfully fetched feed with entries")
                return True
            else:
                self.logger.warning("Feed fetched but contains no entries")
                return False
                
        except Exception as e:
            self.logger.error(f"Error fetching feed: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return False
    
    def get_podcast_info(self) -> Dict:
        """
        Get basic podcast information
        
        Returns:
            dict: Podcast metadata including title, description, etc.
        """
        if not self.feed_data:
            self.logger.warning("Attempted to get podcast info without feed data")
            return {}
            
        try:
            info = {
                'title': self.feed_data.feed.get('title', 'Unknown'),
                'description': self.feed_data.feed.get('description', 'No description available'),
                'link': self.feed_data.feed.get('link', ''),
                'language': self.feed_data.feed.get('language', 'Unknown'),
                'author': self.feed_data.feed.get('author', 'Unknown')
            }
            self.logger.debug(f"Retrieved podcast info: {info}")
            return info
        except Exception as e:
            self.logger.error(f"Error getting podcast info: {str(e)}")
            self.logger.error(f"Traceback: {traceback.format_exc()}")
            return {}
    
    def get_episodes(self) -> List[Dict]:
        """
        Get all episodes from the feed
        
        Returns:
            list: List of dictionaries containing episode information
        """
        if not self.feed_data:
            return []
            
        episodes = []
        for entry in self.feed_data.entries:
            episode = {
                'title': entry.get('title', 'Unknown'),
                'description': entry.get('description', 'No description available'),
                'published': self._parse_date(entry.get('published', '')),
                'duration': entry.get('itunes_duration', 'Unknown'),
                'link': entry.get('link', ''),
                'audio_url': self._get_audio_url(entry)
            }
            episodes.append(episode)
            
        return episodes
    
    def print_episodes(self):
        """
        Print all episodes in a formatted way
        """
        if not self.feed_data:
            print("No feed data available. Call fetch_feed() first.")
            return
            
        podcast_info = self.get_podcast_info()
        print(f"\n=== {podcast_info['title']} ===")
        print(f"Author: {podcast_info['author']}")
        print(f"Language: {podcast_info['language']}")
        print("\nEpisodes:")
        print("-" * 80)
        
        for idx, episode in enumerate(self.get_episodes(), 1):
            print(f"\n{idx}. {episode['title']}")
            print(f"Published: {episode['published']}")
            print(f"Duration: {episode['duration']}")
            print(f"Link: {episode['link']}")
            print(f"Audio URL: {episode['audio_url']}")
            print("-" * 80)
    
    def _parse_date(self, date_str: str) -> str:
        """
        Parse publication date into a consistent format
        """
        try:
            parsed_date = datetime.strptime(date_str, '%a, %d %b %Y %H:%M:%S %z')
            return parsed_date.strftime('%Y-%m-%d %H:%M:%S')
        except:
            return date_str
    
    def _get_audio_url(self, entry: Dict) -> str:
        """
        Extract audio URL from feed entry
        """
        # Try to get enclosure URL (most common location for audio file)
        if 'enclosures' in entry and entry.enclosures:
            return entry.enclosures[0].get('href', '')
            
        # Fallback to media_content
        if 'media_content' in entry and entry.media_content:
            return entry.media_content[0].get('url', '')
            
        return 'No audio URL found'

# Example usage
feed_url = "https://yourcollegeboundkid.com/feed/"  # Keeping original feed URL
#feed_url = "https://feeds.npr.org/500005/podcast.xml"
podcast = PodcastFeed(feed_url)

if podcast.fetch_feed():
    podcast.print_episodes()
else:
    print("\nFeed fetch failed. Check the logs above for detailed error information.")
