using ParseAnthemFile;
using System.Diagnostics;

public class Program
{
    public static void Main()
    {
        CustomConsole.SetLogFilePath("anthem-log.txt");

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

    /**
    TODO: 
    1. Download all files for one org
    2. Save them under the same folder
    3. Create a file of contents
    4. Count the number of rows for each in network file
    5. Count how many in network files are there
    6. Calculate the amount of data to save in the database

    11. Download file for one hospital
    12. Find the data intersection between the two files
     */


    private static void DownloadAndDecompress(ReportingData data)
    {
        int index = 0;
        foreach (ReportingStructure structure in data.ReportingStructure)
        {
            foreach (InNetworkFile infile in structure.InNetworkFiles)
            {
                string location = infile.Location;

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

