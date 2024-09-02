using ParseAnthemFile;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        AnthemFileParser parser = new AnthemFileParser();
        //var data = parser.ParseAnthemFile(@"C:\Projects\BlazorProjects\MedicalTourism\ParseAnthemFile\anthem-partial.json");
        //Console.WriteLine(JsonSerializer.Serialize(data));
        ////parser.InsertReportingDataIntoDatabase(data);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        parser.ParseAnthemFullFile(@"C:\Users\alish\Downloads\2024-08-01_anthem_index.json");
        stopwatch.Stop();
        Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}");
    }

    private static void DownloadAndDecompress(ReportingData data)
    {
        int index = 0;
        foreach (var structure in data.ReportingStructure)
        {
            foreach (var infile in structure.InNetworkFiles)
            {
                var location = infile.Location;

                Console.WriteLine($"Location: {location}");
                // the second file is 6.7GB, so we will only download the first file
                DownloadAndDecompress(location, $"file{index++}.json").Wait();

            }
        }
    }

    public static Task DownloadAndDecompress(string location, string desc)
    {

        return FileDownloader.DownloadAndDecompressGzFileAsync(location, desc);
    }
}

