namespace CutAudio;
using NAudio.Wave;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine(Directory.GetCurrentDirectory());

        // Cut from 30 seconds to 45 seconds
        CutAudio("/Users/macmyths/Projects/Qdrant.Test/wav_files/Harvard01.wav", "/Users/macmyths/Projects/Qdrant.Test/wav_files/output.wav", 30, 45);
    }

    static void CutAudio(string inputPath, string outputPath, int startSeconds, int endSeconds)
    {
        using (var reader = new AudioFileReader(inputPath))
        {
            // Calculate positions in bytes
            var bytesPerSecond = reader.WaveFormat.AverageBytesPerSecond;
            var startPos = startSeconds * bytesPerSecond;
            var endPos = endSeconds * bytesPerSecond;
            var length = endPos - startPos;

            // Create WaveFileWriter with same format as input
            using (var writer = new WaveFileWriter(outputPath, reader.WaveFormat))
            {
                // Skip to start position
                reader.Position = startPos;

                // Create buffer for reading
                var buffer = new byte[1024];
                var bytesRead = 0;
                var totalBytes = 0;

                // Read and write until we reach the end position
                while (totalBytes < length && 
                       (bytesRead = reader.Read(buffer, 0, (int)Math.Min(buffer.Length, length - totalBytes))) > 0)
                {
                    writer.Write(buffer, 0, bytesRead);
                    totalBytes += bytesRead;
                }
            }
        }
    }
}