using Microsoft.Extensions.Logging;
using System.Text;

namespace TestLLM;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _folderPath;
    private readonly string _fileName;
    private readonly int _maxFileSizeInMB;
    private readonly int _maxFilesToKeep;
    private readonly bool _includeTimestamp;
    private readonly bool _includeLogLevel;
    private readonly bool _includeCategory;
    private readonly object _lock = new object();
    
    public FileLoggerProvider(string folderPath, string fileName, int maxFileSizeInMB = 10, 
        int maxFilesToKeep = 30, bool includeTimestamp = true, bool includeLogLevel = true, 
        bool includeCategory = true)
    {
        _folderPath = folderPath;
        _fileName = fileName;
        _maxFileSizeInMB = maxFileSizeInMB;
        _maxFilesToKeep = maxFilesToKeep;
        _includeTimestamp = includeTimestamp;
        _includeLogLevel = includeLogLevel;
        _includeCategory = includeCategory;
        
        // Ensure log directory exists
        EnsureLogDirectoryExists();
    }
    
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(this, categoryName);
    }
    
    public void Dispose()
    {
        // Cleanup if needed
    }
    
    private void EnsureLogDirectoryExists()
    {
        try
        {
            var fullPath = GetFullLogDirectoryPath();
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                System.Diagnostics.Debug.WriteLine($"Created log directory: {fullPath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to create log directory: {ex.Message}");
        }
    }
    
    private string GetFullLogDirectoryPath()
    {
        // Check if the path is absolute (starts with / on Unix/Linux/macOS or has drive letter on Windows)
        if (Path.IsPathRooted(_folderPath))
        {
            return _folderPath;
        }
        else
        {
            // Relative path - combine with app data directory
            return Path.Combine(FileSystem.AppDataDirectory, _folderPath);
        }
    }
    
    private string GetLogFilePath()
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var fileName = _fileName.Replace("{Date}", date);
        var fullDirectoryPath = GetFullLogDirectoryPath();
        return Path.Combine(fullDirectoryPath, fileName);
    }
    
    private void WriteToFile(string message)
    {
        try
        {
            var logFilePath = GetLogFilePath();
            
            lock (_lock)
            {
                // Check file size and rotate if needed
                if (File.Exists(logFilePath))
                {
                    var fileInfo = new FileInfo(logFilePath);
                    if (fileInfo.Length > _maxFileSizeInMB * 1024 * 1024)
                    {
                        RotateLogFiles(logFilePath);
                    }
                }
                
                // Write to file
                File.AppendAllText(logFilePath, message + Environment.NewLine, Encoding.UTF8);
                System.Diagnostics.Debug.WriteLine($"Wrote log entry to: {logFilePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Log file path: {GetLogFilePath()}");
        }
    }
    
    private void RotateLogFiles(string currentLogPath)
    {
        try
        {
            var directory = Path.GetDirectoryName(currentLogPath);
            var fileName = Path.GetFileNameWithoutExtension(currentLogPath);
            var extension = Path.GetExtension(currentLogPath);
            
            // Create backup filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var backupPath = Path.Combine(directory!, $"{fileName}-{timestamp}{extension}");
            
            // Move current file to backup
            if (File.Exists(currentLogPath))
            {
                File.Move(currentLogPath, backupPath);
            }
            
            // Clean up old files
            CleanupOldLogFiles(directory!, fileName, extension);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to rotate log files: {ex.Message}");
        }
    }
    
    private void CleanupOldLogFiles(string directory, string fileName, string extension)
    {
        try
        {
            var pattern = $"{fileName}-*{extension}";
            var files = Directory.GetFiles(directory, pattern)
                .OrderByDescending(f => f)
                .Skip(_maxFilesToKeep);
            
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete old log file {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to cleanup old log files: {ex.Message}");
        }
    }
    
    private class FileLogger : ILogger
    {
        private readonly FileLoggerProvider _provider;
        private readonly string _categoryName;
        
        public FileLogger(FileLoggerProvider provider, string categoryName)
        {
            _provider = provider;
            _categoryName = categoryName;
        }
        
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }
        
        public bool IsEnabled(LogLevel logLevel)
        {
            return true; // Let the provider handle filtering
        }
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            
            var message = new StringBuilder();
            
            // Add timestamp
            if (_provider._includeTimestamp)
            {
                message.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ");
            }
            
            // Add log level
            if (_provider._includeLogLevel)
            {
                message.Append($"[{logLevel}] ");
            }
            
            // Add category
            if (_provider._includeCategory)
            {
                message.Append($"[{_categoryName}] ");
            }
            
            // Add message
            message.Append(formatter(state, exception));
            
            // Add exception details
            if (exception != null)
            {
                message.Append($" Exception: {exception.Message}");
                if (exception.StackTrace != null)
                {
                    message.Append($" StackTrace: {exception.StackTrace}");
                }
            }
            
            _provider.WriteToFile(message.ToString());
        }
    }
} 