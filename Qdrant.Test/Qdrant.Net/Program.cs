using System.Net;
using Grpc.Net.Client;

namespace Qdrant.Net;

class Program
{
    static async Task Main(string[] args)
    {
        var processor = new AudioProcessor();
        //await processor.InitializeCollection();
        
        // Process and upload files
        //await processor.ProcessAndUploadFiles("wav_files");

        // Search example
        Console.WriteLine("Searching for 'canoe' in database");
        var results = await processor.SearchAudioFragments("canoe");

        foreach (var result in results)
        {
            Console.WriteLine($"\nFound 'canoe' in file: {result.Filename}");
            Console.WriteLine($"Full Transcript: {result.Transcript}");

            foreach (var segment in result.Segments)
            {
                if (segment.Text.ToLower().Contains("canoe"))
                {
                    Console.WriteLine($"\nTimestamp: {segment.Start:F2}s - {segment.End:F2}s");
                    Console.WriteLine($"Segment: {segment.Text}");
                }
            }
        }

        Console.WriteLine("\nDatabase search complete");
    }
    
    /**
     * Key differences from the Python version:
Uses C# async/await pattern for asynchronous operations
More structured with proper classes and types
Uses NAudio instead of librosa for audio processing
Uses Whisper.net instead of OpenAI's whisper
Proper error handling and disposal of resources with using statements
More type-safe with strong typing
Note: The MFCC feature extraction is marked as 
TODO because there isn't a direct C# equivalent to librosa's MFCC implementation. 
You'll need to either:
Use a DSP library like Accord.NET
Implement MFCC calculation yourself
Call Python's librosa from C# using Python.NET
Also, you'll need to download and specify the path to the Whisper model file for the transcription to work.
     */
}