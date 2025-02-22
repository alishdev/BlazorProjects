using Microsoft.Extensions.Logging;

namespace Librarian.Crawl;

public class FileCrawler
{
    private readonly string _directoryPath;
    private readonly ILogger<FileCrawler> _logger;

    public FileCrawler(string directoryPath, ILogger<FileCrawler> logger)
    {
        _directoryPath = directoryPath;
        _logger = logger;
    }

    public async Task CrawlDirectory()
    {
        try
        {
            _logger.LogInformation("Starting directory crawl at: {DirectoryPath}", _directoryPath);

            if (!Directory.Exists(_directoryPath))
            {
                _logger.LogError("Directory does not exist: {DirectoryPath}", _directoryPath);
                return;
            }

            var files = Directory.GetFiles(_directoryPath);
            _logger.LogInformation("Found {Count} files in directory", files.Length);

            foreach (var file in files)
            {
                _logger.LogDebug("Processing file: {FilePath}", file);
                try
                {
                    var fileInfo = new FileInfo(file);
                    _logger.LogInformation("File: {FileName}, Size: {Size} bytes, Last Modified: {LastModified}",
                        fileInfo.Name,
                        fileInfo.Length,
                        fileInfo.LastWriteTime);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing file: {FilePath}", file);
                }
            }

            _logger.LogInformation("Completed directory crawl at: {DirectoryPath}", _directoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while crawling directory: {DirectoryPath}", _directoryPath);
            throw;
        }
    }
}