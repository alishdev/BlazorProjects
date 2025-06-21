# YCrawl

A recursive web crawler built with Python that uses Firecrawl to download all pages from a given URL as HTML files.

## Features

- **Recursive crawling**: Automatically discovers and downloads linked pages
- **HTML download**: Saves all pages as HTML files
- **Configurable depth**: Control how deep the crawler goes
- **Same-domain filtering**: Only crawls pages from the same domain
- **Progress tracking**: Shows real-time crawl progress and statistics
- **Crawl reports**: Generates detailed JSON reports of the crawl session
- **Configuration file**: Store API key and settings in a config file

## Installation

1. **Clone or download this repository**

2. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

3. **Get a Firecrawl API key**:
   - Sign up at [Firecrawl](https://firecrawl.dev)
   - Get your API key from the dashboard

4. **Set up your API key**:
   ```bash
   python ycrawl.py --set-api-key YOUR_API_KEY
   ```

5. brew install cairo pango gdk-pixbuf libffi
or Use pdfkit as a Fallback
brew install --cask wkhtmltopdf

## Configuration

The program uses a `config.json` file to store settings. The file is automatically created on first run with default values:

```json
{
  "firecrawl_api_key": "",
  "default_output_dir": "crawled_pages",
  "default_max_depth": 3,
  "wait_for_network_idle": true,
  "include_screenshots": false,
  "include_pdf": false,
  "include_metadata": true
}
```

### Configuration Options

- `firecrawl_api_key`: Your Firecrawl API key
- `default_output_dir`: Default directory for saving HTML files
- `default_max_depth`: Default maximum crawl depth
- `wait_for_network_idle`: Wait for network to be idle before capturing
- `include_screenshots`: Include screenshots in crawl results
- `include_pdf`: Include PDF generation
- `include_metadata`: Include page metadata

## Usage

### Setting up API Key

```bash
# Set API key in config file
python ycrawl.py --set-api-key YOUR_API_KEY

# Or use a custom config file
python ycrawl.py --config-file my_config.json --set-api-key YOUR_API_KEY
```

### Basic Usage

```bash
python ycrawl.py https://example.com
```

### Advanced Usage

```bash
# Override config file settings
python ycrawl.py https://example.com --output-dir ./my_pages --max-depth 5
python ycrawl.py https://alisher.io/ --output-dir ./alisher_pages 

# Use custom config file
python ycrawl.py --config-file my_config.json https://example.com

# Override API key for this run only
python ycrawl.py https://example.com --api-key YOUR_API_KEY
```

### Command Line Options

- `url`: Starting URL to crawl (required)
- `--api-key`: Firecrawl API key (overrides config file)
- `--output-dir`: Directory to save HTML files (overrides config file)
- `--max-depth`: Maximum crawl depth (overrides config file)
- `--config-file`: Configuration file path (default: config.json)
- `--set-api-key`: Set API key in configuration file

## API Key Priority

The program looks for the API key in this order:
1. Command line argument (`--api-key`)
2. Configuration file (`config.json`)
3. Environment variable (`FIRECRAWL_API_KEY`)

## Output

The crawler creates:

1. **HTML files**: Each page is saved as an HTML file in the output directory
2. **Crawl report**: A `crawl_report.json` file with detailed statistics including:
   - Total pages processed
   - Successful/failed downloads
   - Duration
   - URL to filename mapping
   - List of all visited URLs

### Example Output Structure

```
crawled_pages/
├── index.html
├── about.html
├── contact.html
├── products.html
└── crawl_report.json
```

## Example

```bash
# First time setup
python ycrawl.py --set-api-key YOUR_API_KEY

# Crawl a website with depth 2
python ycrawl.py https://example.com --max-depth 2 --output-dir example_site

# Output will show:
# Starting crawl of: https://example.com
# Output directory: example_site
# Max depth: 2
# Config file: config.json
# --------------------------------------------------
# [0] Crawling: https://example.com
#   ✓ Saved: index.html
# [1] Crawling: https://example.com/about
#   ✓ Saved: about.html
# [1] Crawling: https://example.com/contact
#   ✓ Saved: contact.html
# 
# ==================================================
# CRAWL SUMMARY
# ==================================================
# Total pages processed: 3
# Successful downloads: 3
# Failed downloads: 0
# Duration: 5.23 seconds
# Files saved to: example_site
# ==================================================
```

## Requirements

- Python 3.7+
- Firecrawl API key
- Internet connection

## Dependencies

- `firecrawl`: For web scraping and page rendering
- `aiofiles`: For asynchronous file operations
- Standard library modules: `os`, `sys`, `argparse`, `asyncio`, `urllib.parse`, `pathlib`, `time`, `typing`, `json`, `re`

## Notes

- The crawler respects the same-domain policy and only crawls pages from the starting URL's domain
- Files are named based on the URL path, with unsafe characters replaced
- The crawler waits for network idle before capturing page content (configurable)
- You can interrupt the crawl at any time with Ctrl+C
- Configuration file is automatically created on first run

## Troubleshooting

1. **"firecrawl package not found"**: Run `pip install -r requirements.txt`
2. **"No API key provided"**: Set your API key using `python ycrawl.py --set-api-key YOUR_API_KEY`
3. **"Error initializing Firecrawl"**: Check your API key and internet connection
4. **Permission errors**: Ensure you have write permissions to the output directory
5. **Config file errors**: Delete `config.json` and run the program again to recreate it

## License

This project is open source and available under the MIT License. 