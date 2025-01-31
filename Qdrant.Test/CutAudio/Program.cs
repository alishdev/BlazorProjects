using System.Diagnostics;

namespace CutAudio;
using NAudio.Wave;
using NAudio.Lame;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine(Directory.GetCurrentDirectory());

            // Cut from 30 seconds to 45 seconds
            CutAudioGeneric("/Users/macmyths/BlazorProjects/Qdrant.Test/mp3_files/GwynethGEORGETOWNPart1.mp3",
                "/Users/macmyths/BlazorProjects/Qdrant.Test/mp3_files/output.mp3", 30, 45);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static void CutAudioGeneric(string inputPath, string outputPath, int startSeconds, int endSeconds)
    {
        var duration = endSeconds - startSeconds;
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            // -acodec copy preserves the original audio codec (MP3 or WAV)
            Arguments = $"-i \"{inputPath}\" -ss {startSeconds} -t {duration} -acodec copy \"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        
        try
        {
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new Exception($"FFmpeg failed with exit code {process.ExitCode}: {error}");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new Exception("Failed to cut audio file. Make sure FFmpeg is installed and available in PATH.", ex);
        }
        catch (InvalidOperationException)
        {
            throw new Exception("FFmpeg is not installed or not found in PATH. Please install FFmpeg first.");
        }
    }

    public static void CutMp3(string inputPath, string outputPath, int startSeconds, int endSeconds)
    {
        using (var reader = new Mp3FileReader(inputPath))
        {
            // Calculate positions in bytes
            var sampleRate = reader.WaveFormat.SampleRate;
            var bytesPerSecond = reader.WaveFormat.AverageBytesPerSecond;
            var startPos = startSeconds * bytesPerSecond;
            var endPos = endSeconds * bytesPerSecond;
            var length = endPos - startPos;

            // Skip to start position
            reader.Position = startPos;

            // Create temporary WAV file
            var tempWavPath = Path.GetTempFileName();
            try
            {
                // First convert the section to WAV
                using (var writer = new WaveFileWriter(tempWavPath, reader.WaveFormat))
                {
                    // Create buffer for reading
                    var buffer = new byte[4096];
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

                // Then convert WAV to MP3
                using (var reader2 = new WaveFileReader(tempWavPath))
                using (var writer2 = new LameMP3FileWriter(outputPath, reader2.WaveFormat, LAMEPreset.VBR_90))
                {
                    reader2.CopyTo(writer2);
                }
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempWavPath))
                {
                    File.Delete(tempWavPath);
                }
            }
        }
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