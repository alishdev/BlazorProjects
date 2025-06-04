using Microsoft.Extensions.Logging;

namespace CampCopilot.Services;

public interface IFileService
{
    Task<string> SaveAudioFileAsync(byte[] data, string filename);
}

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SaveAudioFileAsync(byte[] data, string filename)
    {
        try
        {
            string savePath;
            
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Use the app's Documents directory for iOS
                savePath = Path.Combine(FileSystem.AppDataDirectory, "Documents");
            }
            else
            {
                // Use Downloads folder for other platforms
                savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            
            _logger.LogInformation($"Save directory path: {savePath}");

            // Ensure directory exists
            if (!Directory.Exists(savePath))
            {
                _logger.LogInformation("Creating save directory");
                Directory.CreateDirectory(savePath);
            }

            // Create the full file path
            string filePath = Path.Combine(savePath, filename);
            _logger.LogInformation($"Saving file to: {filePath}");

            // Write the file
            await File.WriteAllBytesAsync(filePath, data);
            _logger.LogInformation($"File saved successfully: {filePath}");

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving audio file");
            throw;
        }
    }
} 