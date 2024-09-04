using Downloader;
using System.ComponentModel;
using System.IO.Compression;

public class FileDownloader
{
    public static async Task DownloadAndDecompressGzFileAsync(string url, string outputFilePath)
    {
        Console.WriteLine($"Downloading and decompressing {url} to {outputFilePath}");
        using (HttpClient client = new HttpClient())
        {
            // Add necessary headers
            client.DefaultRequestHeaders.Add("User-Agent", "MedicalTourism");
            client.DefaultRequestHeaders.Referrer = new Uri("https://alisher.io");

            // Download the gz file
            byte[] gzData = await client.GetByteArrayAsync(url);

            // Decompress the gz file
            using (MemoryStream gzStream = new MemoryStream(gzData))
            {
                gzStream.Seek(0, SeekOrigin.Begin);
                using (GZipStream decompressionStream = new GZipStream(gzStream, CompressionMode.Decompress))
                using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create))
                {
                    await decompressionStream.CopyToAsync(outputFileStream);
                }
            }
        }
    }

    private static void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        DownloadService service = (DownloadService)sender;
        string url = service.Package.FileName;
        if (e.Cancelled)
        {
            CustomConsole.WriteLine($"CANCELED: {url}");
        }
        else if (e.Error != null)
        {
            CustomConsole.WriteLine($"{e.Error}: {url}");
        }
        else
        {
            CustomConsole.WriteLine($"{url} DONE");
        }
    }

    public static async Task DownloadFileAsync(string url, string outputFilePath)
    {
        CustomConsole.WriteLine($"Downloading {url} to {outputFilePath}");
        DownloadConfiguration downloadOpt = new DownloadConfiguration()
        {
            ChunkCount = 8, // file parts to download, the default value is 1
            ParallelDownload = true // download parts of the file as parallel or not. The default value is false
        };

        DownloadService downloader = new DownloadService(downloadOpt);

        downloader.DownloadFileCompleted += OnDownloadFileCompleted;

        /*string file = @"Your_Path\fileName.zip";
        string url = @"https://file-examples.com/fileName.zip";
        await downloader.DownloadFileTaskAsync(url, file);*/

        DirectoryInfo path = new DirectoryInfo(outputFilePath);
        // download into "outputFilePath\fileName.zip"
        downloader.DownloadFileTaskAsync(url, path).Wait();
        CustomConsole.WriteLine($"Downloaded {url}");
    }
}
