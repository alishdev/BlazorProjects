using System.Diagnostics;
using NAudio.Wave;

namespace PodcastChat.Logic;

public class CutAudio
{
    public static void CutAudioGeneric(string inputPath, string outputPath, int startSeconds, int endSeconds)
    {
        if (File.Exists(outputPath))
            return;
        
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
}