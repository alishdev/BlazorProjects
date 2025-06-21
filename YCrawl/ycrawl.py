#!/usr/bin/env python3
"""
YCrawl - A recursive web crawler using Firecrawl
Downloads all pages from a given URL as HTML files and PDFs
"""

import os
import sys
import argparse
import asyncio
import aiofiles
from urllib.parse import urljoin, urlparse
from pathlib import Path
import time
from typing import Set, Dict, List
import json
import requests

try:
    from firecrawl import FirecrawlApp
except ImportError:
    print("Error: firecrawl package not found. Please install it with: pip install firecrawl")
    sys.exit(1)

# Try to import PDF libraries, but don't fail if they're not available
PDFKIT_AVAILABLE = False
WEASYPRINT_AVAILABLE = False
PLAYWRIGHT_AVAILABLE = False

try:
    import pdfkit
    PDFKIT_AVAILABLE = True
    print("✓ pdfkit available for PDF generation")
except ImportError:
    print("⚠ pdfkit not available. Install with: pip install pdfkit")

try:
    from playwright.sync_api import sync_playwright
    PLAYWRIGHT_AVAILABLE = True
    print("✓ playwright available for PDF generation")
except ImportError:
    print("⚠ playwright not available. Install with: pip install playwright && playwright install")

# Don't import WeasyPrint at module level to avoid crashes
# We'll import it only when needed inside the function

if not PLAYWRIGHT_AVAILABLE and not PDFKIT_AVAILABLE:
    print("⚠ No PDF libraries available. PDF generation will be disabled.")
    print("Install playwright: pip install playwright && playwright install")


class ConfigManager:
    """Manages configuration file for YCrawl"""
    
    def __init__(self, config_file: str = "config.json"):
        self.config_file = Path(config_file)
        self.config = self.load_config()
    
    def load_config(self) -> Dict:
        """Load configuration from file or create default"""
        if self.config_file.exists():
            try:
                with open(self.config_file, 'r') as f:
                    config = json.load(f)
                return config
            except (json.JSONDecodeError, IOError) as e:
                print(f"Warning: Error reading config file: {e}")
                return self.get_default_config()
        else:
            # Create default config file
            default_config = self.get_default_config()
            self.save_config(default_config)
            print(f"Created default config file: {self.config_file}")
            return default_config
    
    def get_default_config(self) -> Dict:
        """Get default configuration"""
        return {
            "firecrawl_api_key": "",
            "default_output_dir": "crawled_pages",
            "default_max_depth": 3,
            "wait_for_network_idle": True,
            "include_screenshots": False,
            "include_pdf": False,
            "include_metadata": True
        }
    
    def save_config(self, config: Dict = None):
        """Save configuration to file"""
        if config is None:
            config = self.config
        
        try:
            with open(self.config_file, 'w') as f:
                json.dump(config, f, indent=2)
        except IOError as e:
            print(f"Error saving config file: {e}")
    
    def get(self, key: str, default=None):
        """Get configuration value"""
        return self.config.get(key, default)
    
    def set(self, key: str, value):
        """Set configuration value and save"""
        self.config[key] = value
        self.save_config()
    
    def update_api_key(self, api_key: str):
        """Update API key in config file"""
        self.set("firecrawl_api_key", api_key)
        print(f"API key updated in {self.config_file}")


class YCrawl:
    def __init__(self, api_key: str = None, output_dir: str = None, max_depth: int = None, config_file: str = "config.json"):
        """
        Initialize YCrawl with Firecrawl configuration
        
        Args:
            api_key: Firecrawl API key (optional, can be set via config file or environment variable)
            output_dir: Directory to save downloaded HTML files
            max_depth: Maximum depth for recursive crawling
            config_file: Path to configuration file
        """
        # Load configuration
        self.config_manager = ConfigManager(config_file)
        
        # Priority: command line > config file > environment variable
        self.api_key = api_key or self.config_manager.get("firecrawl_api_key") or os.getenv('FIRECRAWL_API_KEY')
        if not self.api_key:
            print("Warning: No API key provided. Set it in config.json, FIRECRAWL_API_KEY environment variable, or pass --api-key")
            print("You can set the API key using: python ycrawl.py --set-api-key YOUR_API_KEY")
        
        self.output_dir = Path(output_dir or self.config_manager.get("default_output_dir"))
        self.output_dir.mkdir(exist_ok=True)
        
        self.max_depth = max_depth or self.config_manager.get("default_max_depth")
        self.visited_urls: Set[str] = set()
        self.url_to_filename: Dict[str, str] = {}
        self.crawl_stats = {
            'total_pages': 0,
            'successful_downloads': 0,
            'failed_downloads': 0,
            'start_time': None,
            'end_time': None
        }
        
        # Initialize Firecrawl client
        try:
            self.firecrawl = FirecrawlApp(api_key=self.api_key)
        except Exception as e:
            print(f"Error initializing Firecrawl: {e}")
            sys.exit(1)
    
    def sanitize_filename(self, url: str) -> str:
        """Convert URL to a safe filename"""
        parsed = urlparse(url)
        # Remove protocol and domain, keep path
        path = parsed.path.strip('/')
        if not path:
            path = 'index'
        
        # Replace unsafe characters
        filename = path.replace('/', '_').replace('?', '_').replace('&', '_').replace('=', '_')
        filename = ''.join(c for c in filename if c.isalnum() or c in '._-')
        
        # Ensure it's not too long
        if len(filename) > 100:
            filename = filename[:100]
        
        # Add .html extension if not present
        if not filename.endswith('.html'):
            filename += '.html'
        
        return filename
    
    def download_file(self, file_url: str):
        """Download a file and save it to the output directory"""
        local_filename = file_url.split('/')[-1].split('?')[0]
        local_path = self.output_dir / local_filename
        if local_path.exists():
            print(f"  ✓ File already downloaded: {local_filename}")
            return
        try:
            with requests.get(file_url, stream=True, timeout=30) as r:
                r.raise_for_status()
                with open(local_path, 'wb') as f:
                    for chunk in r.iter_content(chunk_size=8192):
                        f.write(chunk)
            print(f"  ✓ File downloaded: {local_filename}")
        except Exception as e:
            print(f"  ✗ Failed to download {file_url}: {e}")
    
    def download_page(self, url: str, depth: int = 0) -> List[str]:
        """
        Download a single page and extract links for further crawling
        
        Args:
            url: URL to download
            depth: Current crawl depth
            
        Returns:
            List of URLs found on the page
        """
        if depth > self.max_depth or url in self.visited_urls:
            return []
        
        self.visited_urls.add(url)
        self.crawl_stats['total_pages'] += 1
        
        print(f"[{depth}] Crawling: {url}")
        
        try:
            # Use Firecrawl to get the page content
            # Set up parameters according to Firecrawl API
            params = {
                "formats": ["html"],
                "wait_for": 2000 if self.config_manager.get("wait_for_network_idle", True) else None,
                "timeout": 30000
            }
            
            # Remove None values
            params = {k: v for k, v in params.items() if v is not None}
            
            response = self.firecrawl.scrape_url(
                url=url,
                **params
            )
            
            if response and hasattr(response, 'html') and response.html:
                # Save HTML content (skip this part)
                # filename = self.sanitize_filename(url)
                # filepath = self.output_dir / filename
                # 
                # with open(filepath, 'w', encoding='utf-8') as f:
                #     f.write(response.html)
                
                # Save as PDF if enabled
                if self.config_manager.get("include_pdf", True):
                    pdf_filename = self.sanitize_filename(url).rsplit('.', 1)[0] + '.pdf'
                    pdf_filepath = self.output_dir / pdf_filename
                    pdf_saved = False
                    
                    # Try playwright first (more reliable on macOS)
                    if PLAYWRIGHT_AVAILABLE and not pdf_saved:
                        try:
                            with sync_playwright() as playwright:
                                browser = playwright.chromium.launch()
                                page = browser.new_page()
                                page.goto(url)
                                page.pdf(path=str(pdf_filepath))
                                browser.close()
                            print(f"  ✓ PDF saved: {pdf_filename}")
                            pdf_saved = True
                        except Exception as e:
                            print(f"  ⚠ playwright failed: {e}")
                    
                    # Try pdfkit as fallback
                    if PDFKIT_AVAILABLE and not pdf_saved:
                        try:
                            pdfkit.from_string(response.html, str(pdf_filepath))
                            print(f"  ✓ PDF saved: {pdf_filename}")
                            pdf_saved = True
                        except Exception as e:
                            print(f"  ⚠ pdfkit failed: {e}")
                    
                    # Try WeasyPrint as fallback (import only when needed)
                    if not pdf_saved:
                        try:
                            from weasyprint import HTML
                            HTML(string=response.html, base_url=url).write_pdf(str(pdf_filepath))
                            print(f"  ✓ PDF saved: {pdf_filename}")
                            pdf_saved = True
                        except ImportError:
                            print("  ⚠ WeasyPrint not available")
                        except Exception as e:
                            print(f"  ⚠ WeasyPrint failed: {e}")
                    
                    if not pdf_saved:
                        print("  ✗ PDF not saved: All PDF libraries failed or unavailable")
                        print("    Install playwright: pip install playwright && playwright install")
                        print("    Or install wkhtmltopdf for pdfkit: download from wkhtmltopdf.org")
                        print("    Or install WeasyPrint dependencies: brew install cairo pango gdk-pixbuf libffi")
                
                # Track the page as successfully processed (even without HTML file)
                self.url_to_filename[url] = pdf_filename if self.config_manager.get("include_pdf", True) else "no_file"
                self.crawl_stats['successful_downloads'] += 1
                print(f"  ✓ Processed: {url}")
                
                # Extract links for further crawling
                links = self.extract_links(response.html, url)
                # Download files if they match certain extensions
                file_exts = ('.txt', '.doc', '.docx', '.xls', '.xlsx', '.pdf')
                for link in links:
                    if link.lower().endswith(file_exts):
                        self.download_file(link)
                # Return only non-file links for further crawling
                crawl_links = [l for l in links if not l.lower().endswith(file_exts)]
                return crawl_links
            else:
                print(f"  ✗ No HTML content received for {url}")
                self.crawl_stats['failed_downloads'] += 1
                return []
                
        except Exception as e:
            print(f"  ✗ Error downloading {url}: {e}")
            self.crawl_stats['failed_downloads'] += 1
            return []
    
    def extract_links(self, html: str, base_url: str) -> List[str]:
        """Extract all links from HTML content"""
        import re
        
        # Simple regex to extract href attributes
        href_pattern = r'href=["\']([^"\']+)["\']'
        links = re.findall(href_pattern, html)
        
        # Filter and normalize links
        valid_links = []
        base_domain = urlparse(base_url).netloc
        
        for link in links:
            # Skip anchors, javascript, mailto, etc.
            if link.startswith(('#', 'javascript:', 'mailto:', 'tel:')):
                continue
            
            # Make relative URLs absolute
            absolute_url = urljoin(base_url, link)
            
            # Only include links from the same domain
            if urlparse(absolute_url).netloc == base_domain:
                valid_links.append(absolute_url)
        
        return list(set(valid_links))  # Remove duplicates
    
    def crawl_recursive(self, start_url: str, depth: int = 0):
        """
        Recursively crawl URLs starting from start_url
        
        Args:
            start_url: The URL to start crawling from
            depth: Current crawl depth
        """
        if depth > self.max_depth:
            return
        
        # Download current page and get links
        links = self.download_page(start_url, depth)
        
        # Recursively crawl found links
        for link in links:
            if link not in self.visited_urls:
                self.crawl_recursive(link, depth + 1)
    
    def crawl(self, start_url: str):
        """Main crawling method"""
        self.crawl_stats['start_time'] = time.time()
        print(f"Starting crawl of: {start_url}")
        print(f"Output directory: {self.output_dir}")
        print(f"Max depth: {self.max_depth}")
        print(f"Config file: {self.config_manager.config_file}")
        print("-" * 50)
        
        self.crawl_recursive(start_url)
        
        self.crawl_stats['end_time'] = time.time()
        self.print_summary()
        self.save_crawl_report()
    
    def print_summary(self):
        """Print crawl summary"""
        duration = self.crawl_stats['end_time'] - self.crawl_stats['start_time']
        
        print("\n" + "=" * 50)
        print("CRAWL SUMMARY")
        print("=" * 50)
        print(f"Total pages processed: {self.crawl_stats['total_pages']}")
        print(f"Successful downloads: {self.crawl_stats['successful_downloads']}")
        print(f"Failed downloads: {self.crawl_stats['failed_downloads']}")
        print(f"Duration: {duration:.2f} seconds")
        print(f"Files saved to: {self.output_dir}")
        print("=" * 50)
    
    def save_crawl_report(self):
        """Save crawl report as JSON"""
        report = {
            'crawl_stats': self.crawl_stats,
            'url_to_filename': self.url_to_filename,
            'visited_urls': list(self.visited_urls)
        }
        
        report_path = self.output_dir / 'crawl_report.json'
        with open(report_path, 'w') as f:
            json.dump(report, f, indent=2)
        
        print(f"Crawl report saved to: {report_path}")


def main():
    """Main function"""
    parser = argparse.ArgumentParser(
        description="YCrawl - Recursive web crawler using Firecrawl",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python ycrawl.py https://example.com
  python ycrawl.py https://example.com --output-dir ./my_pages --max-depth 5
  python ycrawl.py --set-api-key YOUR_API_KEY
  python ycrawl.py --config-file my_config.json https://example.com
        """
    )
    
    parser.add_argument('url', nargs='?', help='Starting URL to crawl')
    parser.add_argument('--api-key', help='Firecrawl API key (overrides config file)')
    parser.add_argument('--output-dir', help='Output directory for HTML files (overrides config file)')
    parser.add_argument('--max-depth', type=int, help='Maximum crawl depth (overrides config file)')
    parser.add_argument('--config-file', default='config.json', help='Configuration file path (default: config.json)')
    parser.add_argument('--set-api-key', help='Set API key in configuration file')
    
    args = parser.parse_args()
    
    # Handle setting API key
    if args.set_api_key:
        config_manager = ConfigManager(args.config_file)
        config_manager.update_api_key(args.set_api_key)
        return
    
    # Check if URL is provided
    if not args.url:
        parser.print_help()
        print("\nError: URL is required for crawling")
        print("Use --set-api-key to configure your API key first")
        sys.exit(1)
    
    # Validate URL
    if not args.url.startswith(('http://', 'https://')):
        print("Error: URL must start with http:// or https://")
        sys.exit(1)
    
    # Create and run crawler
    crawler = YCrawl(
        api_key=args.api_key,
        output_dir=args.output_dir,
        max_depth=args.max_depth,
        config_file=args.config_file
    )
    
    try:
        crawler.crawl(args.url)
    except KeyboardInterrupt:
        print("\nCrawl interrupted by user")
    except Exception as e:
        print(f"Error during crawl: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main() 