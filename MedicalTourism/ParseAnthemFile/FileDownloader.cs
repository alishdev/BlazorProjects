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
}
