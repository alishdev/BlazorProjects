# to download the podcast episodes

import asyncio
import argparse
from taddy_client import TaddyClient

def run_taddy_client(podcast_name):
    client = TaddyClient()
    try:
        asyncio.run(client.main(podcast_name))
    except KeyboardInterrupt:
        client.logger.info("Process interrupted by user")
    except Exception as e:
        client.logger.error(f"Unexpected error: {str(e)}")
    finally:
        client.logger.info("Process completed")

if __name__ == "__main__":
    # Set up argument parser
    parser = argparse.ArgumentParser(description='Podcast downloader and processor')
    parser.add_argument('--d', metavar='PODCAST_NAME', type=str, help='Download episodes for the specified podcast')
    
    # Parse arguments
    args = parser.parse_args()
    
    # Run downloader if --d flag is present with podcast name
    if args.d:
        run_taddy_client(args.d)
    else:
        print("Usage: python main.py --d 'Podcast Name'")
        print("Example: python main.py --d 'Your College Bound Kid'")
