using Microsoft.Extensions.Logging;
using Librarian.Crawl;

string logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", $"crawler-{DateTime.Now:yyyy-MM-dd}.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddFile(logPath);
});

var logger = loggerFactory.CreateLogger<FileCrawler>();

string directoryPath = Directory.GetCurrentDirectory();
logger.LogInformation("Crawling directory: {DirectoryPath}", directoryPath);
logger.LogInformation("Starting crawler");

var crawler = new FileCrawler(directoryPath, logger);
await crawler.CrawlDirectory();