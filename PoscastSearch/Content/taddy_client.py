import os
import asyncio
import logging
from dotenv import load_dotenv
from gql import gql, Client
from gql.transport.aiohttp import AIOHTTPTransport
from mp3downloader import MP3Downloader  # Import the MP3Downloader

class TaddyClient:
    def __init__(self):
        # Set up logging to both file and console
        self.setup_logging()
        
        # Load environment variables
        load_dotenv()
        
        # GraphQL endpoint
        self.endpoint_url = "https://api.taddy.org/"
        
        # Headers for requests
        self.headers = {
            'Content-Type': 'application/json',
            'User-Agent': 'Example App',
            'X-USER-ID': os.getenv('TADDY_USER_ID'),
            'X-API-KEY': os.getenv('TADDY_API_KEY'),
        }
        
        # Set up GraphQL client with async transport
        transport = AIOHTTPTransport(
            url=self.endpoint_url,
            headers=self.headers
        )
        self.client = Client(
            transport=transport,
            fetch_schema_from_transport=True
        )
        self.mp3_downloader = MP3Downloader()  # Initialize the downloader

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
                logging.FileHandler('logs/podcast_downloader.log'),
                logging.StreamHandler()
            ]
        )
        
        self.logger = logging.getLogger(__name__)

    async def graphql_request(self, query, variables=None):
        """
        Make a GraphQL request
        
        Args:
            query: GraphQL query string
            variables: Optional variables for the query
            
        Returns:
            dict: Response data
        """
        try:
            self.logger.info("Making GraphQL request...")
            self.logger.info(f"Variables: {variables}")
            result = await self.client.execute_async(query, variable_values=variables)
            self.logger.info(f"Result: {result}")
            return result
        except Exception as e:
            self.logger.error(f"Error in graphql_request: {str(e)}")
            self.logger.error(f"Error type: {type(e)}")
            self.logger.error(f"Error details: {e.__dict__}")
            return None

    # Search query to find podcast
    SEARCH_PODCAST_QUERY = gql("""
        query searchForTerm($term: String!) {
            searchForTerm(
                term: $term,
                page: 1,
                limitPerPage:10,
                filterForTypes: [PODCASTSERIES]
            ) {
                searchId
                podcastSeries {
                    uuid
                    name
                    rssUrl
                }
            }
        }
    """)

    # Query to get podcast episodes with pagination
    GET_EPISODES_QUERY = gql("""
        query getPodcastSeries($uuid: ID!, $page: Int!, $limitPerPage: Int!) {
            getPodcastSeries(uuid: $uuid) {
                uuid
                name
                episodes(sortOrder: OLDEST, page: $page, limitPerPage: $limitPerPage) {
                    uuid
                    name
                    datePublished
                    audioUrl
                    duration
                    episodeNumber
                }
            }
        }
    """)

    # Query to get podcast details
    GET_PODCAST_QUERY = gql("""
        query getPodcastSeries($uuid: ID!) {
            getPodcastSeries(uuid: $uuid) {
                uuid
                name
                rssUrl
                description
                imageUrl
            }
        }
    """)

    # Simple test query
    TEST_QUERY = gql("""
        query {
            getPodcastSeries(uuid: "example-uuid") {
                uuid
                name
                rssUrl
            }
        }
    """)

    # GraphQL Queries
    SEARCH_FOR_TERM_QUERY = gql("""
        query searchForTerm(
            $term: String, 
            $page: Int, 
            $limitPerPage: Int, 
            $filterForTypes: [TaddyType], 
            $filterForCountries: [Country], 
            $filterForLanguages: [Language], 
            $filterForGenres: [Genre], 
            $filterForSeriesUuids: [ID], 
            $filterForNotInSeriesUuids: [ID], 
            $isExactPhraseSearchMode: Boolean, 
            $isSafeMode: Boolean, 
            $searchResultsBoostType: SearchResultBoostType
        ) {
            searchForTerm(
                term: $term,
                page: $page,
                limitPerPage: $limitPerPage,
                filterForTypes: $filterForTypes,
                filterForCountries: $filterForCountries,
                filterForLanguages: $filterForLanguages,
                filterForGenres: $filterForGenres,
                filterForSeriesUuids: $filterForSeriesUuids,
                filterForNotInSeriesUuids: $filterForNotInSeriesUuids,
                isExactPhraseSearchMode: $isExactPhraseSearchMode,
                isSafeMode: $isSafeMode,
                searchResultsBoostType: $searchResultsBoostType
            ) {
                searchId
                podcastSeries {
                    uuid
                    name
                    rssUrl
                    itunesId
                }
                podcastEpisodes {
                    uuid
                    guid
                    name
                    audioUrl
                }
            }
        }
    """)

    GET_PODCASTSERIES = gql("""
        query getPodcastSeries($uuid: ID) {
            getPodcastSeries(uuid: $uuid) {
                uuid
                hash
                name
                description
                imageUrl
                datePublished
                language
                seriesType
                contentType
                isExplicitContent
                copyright
                websiteUrl
                rssUrl
                rssOwnerName
                rssOwnerPublicEmail
                authorName
                isCompleted
                isBlocked
                itunesId
                genres
                childrenHash
                itunesInfo {
                    uuid
                    publisherId
                    publisherName
                    baseArtworkUrl
                    baseArtworkUrlOf(size: 640)
                }
            }
        }
    """)

    GET_PODCASTEPISODE = gql("""
        query getPodcastEpisode($uuid: ID) {
            getPodcastEpisode(uuid: $uuid) {
                uuid
                hash
                name
                description
                imageUrl
                datePublished
                guid
                subtitle
                audioUrl
                videoUrl
                fileLength
                fileType
                duration
                episodeType
                seasonNumber
                episodeNumber
                websiteUrl
                isExplicitContent
                isRemoved
                podcastSeries {
                    uuid
                    name
                    rssUrl
                    itunesId
                }
            }
        }
    """)

    GET_ITUNESINFO = gql("""
        query getItunesInfo($uuid: ID) {
            getItunesInfo(uuid: $uuid) {
                uuid
                hash
                subtitle
                summary
                baseArtworkUrl
                publisherId
                publisherName
                country
                podcastSeries {
                    uuid
                    name
                    rssUrl
                    itunesId
                }
            }
        }
    """)

    GET_COMICSERIES = gql("""
        query GetComicSeries($uuid: ID) {
            getComicSeries(uuid: $uuid) {
                uuid
                name
                description
                status
                hash
                issuesHash
                datePublished
                coverImageAsString
                bannerImageAsString
                thumbnailImageAsString
                tags
                genres
                language
                contentRating
                seriesType
                seriesLayout
                sssUrl
                sssOwnerName
                sssOwnerPublicEmail
                copyright
                isBlocked
                hostingProvider {
                    uuid
                    sssUrl
                }
                scopesForExclusiveContent
            }
        }
    """)

    GET_COMICISSUE = gql("""
        query GetComicIssue($uuid: ID) {
            getComicIssue(uuid: $uuid) {
                uuid
                seriesUuid
                name
                creatorNote
                pushNotificationMessage
                hash
                storiesHash
                datePublished
                bannerImageAsString
                thumbnailImageAsString
                stories {
                    uuid
                    hash
                    storyImageAsString
                }
                position
                scopesForExclusiveContent
                dateExclusiveContentIsAvailable
                isRemoved
                isBlocked
            }
        }
    """)

    GET_CREATOR = gql("""
        query GetCreator($uuid: ID) {
            getCreator(uuid: $uuid) {
                uuid
                name
                bio
                hash
                contentHash
                avatarImageAsString
                tags
                country
                linksAsString
                sssUrl
                sssOwnerName
                sssOwnerPublicEmail
                copyright
                isBlocked
            }
        }
    """)

    GET_CREATORCONTENT = gql("""
        query GetCreatorContent($uuid: ID) {
            getCreatorContent(uuid: $uuid) {
                hash
                creatorUuid
                contentUuid
                contentType
                roles
                position
                contentPosition
            }
        }
    """)

    GET_HOSTINGPROVIDER = gql("""
        query GetHostingProvider($uuid: ID) {
            getHostingProvider(uuid: $uuid) {
                uuid
                hash
                oauth {
                    uuid
                    signupUrl
                    authorizeUrl
                    tokenUrl
                    newAccessTokenUrl
                    newRefreshTokenUrl
                    newContentTokenUrl
                    instructionsUrl
                }
                sssUrl
                sssOwnerName
                sssOwnerPublicEmail
                isBlocked
            }
        }
    """)

# Example usage
if __name__ == "__main__":
    client = TaddyClient()
    
    # Move main() outside the if block and fix the self reference
    async def main(podcast_name):
        # First, search for the podcast
        search_variables = {
            "term": podcast_name
        }
        
        client.logger.info(f"Searching for podcast: {podcast_name}")
        search_result = await client.graphql_request(client.SEARCH_PODCAST_QUERY, search_variables)
        
        if search_result and 'searchForTerm' in search_result:
            podcasts = search_result['searchForTerm']['podcastSeries']
            if podcasts:
                # Get the first matching podcast
                podcast = podcasts[0]
                client.logger.info(f"\nFound podcast: {podcast['name']}")
                client.logger.info(f"UUID: {podcast['uuid']}")
                
                # Download episodes from pages 1 to 20
                for page in range(1, 21):
                    client.logger.info(f"\nProcessing page {page} of episodes...")
                    
                    # Get episodes using the podcast UUID with pagination
                    episodes_variables = {
                        "uuid": podcast['uuid'],
                        "page": page,
                        "limitPerPage": 25
                    }
                    
                    episodes_result = await client.graphql_request(client.GET_EPISODES_QUERY, episodes_variables)
                    
                    if episodes_result and 'getPodcastSeries' in episodes_result:
                        series = episodes_result['getPodcastSeries']
                        episodes = series['episodes']
                        
                        if not episodes:
                            client.logger.info(f"No more episodes found on page {page}")
                            break
                        
                        client.logger.info(f"Found {len(episodes)} episodes on page {page}")
                        client.logger.info("-" * 50)
                        
                        for episode in episodes:
                            client.logger.info(f"\nProcessing Episode {episode.get('episodeNumber', 'N/A')}:")
                            client.logger.info(f"Title: {episode['name']}")
                            
                            # Create a clean filename from episode number and name
                            filename = f"{podcast_name}_{episode.get('episodeNumber', '000')}_{episode['name']}"
                            
                            # Download the episode
                            result = client.mp3_downloader.download(
                                url=episode['audioUrl'],
                                output_dir="podcast_episodes",
                                filename=filename
                            )
                            
                            if result:
                                client.logger.info(f"Successfully downloaded to: {result}")
                            else:
                                client.logger.error(f"Failed to download episode")
                            client.logger.info("-" * 50)
                    else:
                        client.logger.error(f"Failed to fetch episodes for page {page}")
            else:
                client.logger.warning("No podcasts found")
        else:
            client.logger.error("Search failed")
    
    # Run the async main function with command line argument
    import sys
    if len(sys.argv) > 1:
        podcast_name = sys.argv[1]
        asyncio.run(main(podcast_name))
    else:
        print("Please provide a podcast name as an argument") 