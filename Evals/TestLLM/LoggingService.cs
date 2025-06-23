using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace TestLLM;

public static class LoggingService
{
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;
    private static IConfiguration? _configuration;
    private static FileLoggerProvider? _fileLoggerProvider;
    
    public static void Initialize()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== LOGGING SERVICE INITIALIZATION START ===");
            
            // Load logging configuration
            LoadLoggingConfiguration();
            
            System.Diagnostics.Debug.WriteLine("Configuration loaded, creating logger factory...");
            
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddDebug()
                    .AddConsole()
                    .SetMinimumLevel(GetMinimumLogLevel());
                
                // Apply configuration from logging_config.json
                if (_configuration != null)
                {
                    System.Diagnostics.Debug.WriteLine("Adding configuration to builder...");
                    builder.AddConfiguration(_configuration.GetSection("Logging"));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Configuration is null, skipping configuration...");
                }
                
                // Add file logging if configured
                AddFileLogging(builder);
            });
            
            _logger = _loggerFactory.CreateLogger("TestLLM");
            _logger.LogInformation("Logging service initialized");
            
            System.Diagnostics.Debug.WriteLine("=== LOGGING SERVICE INITIALIZATION COMPLETE ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== LOGGING SERVICE INITIALIZATION FAILED ===");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Fallback to basic logging if configuration fails
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddDebug()
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Information);
            });
            
            _logger = _loggerFactory.CreateLogger("TestLLM");
            _logger.LogError(ex, "Failed to initialize logging with configuration, using fallback");
        }
    }
    
    private static void AddFileLogging(ILoggingBuilder builder)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== ADDING FILE LOGGING ===");
            
            if (_configuration != null)
            {
                System.Diagnostics.Debug.WriteLine("Configuration is not null, checking for file section...");
                var fileSection = _configuration.GetSection("Logging:File");
                System.Diagnostics.Debug.WriteLine($"File section exists: {fileSection.Exists()}");
                
                if (fileSection.Exists())
                {
                    var folderPath = fileSection["FolderPath"] ?? "logs";
                    var fileName = fileSection["FileName"] ?? "testllm-{Date}.log";
                    var maxFileSizeInMB = int.TryParse(fileSection["MaxFileSizeInMB"], out var size) ? size : 10;
                    var maxFilesToKeep = int.TryParse(fileSection["MaxFilesToKeep"], out var keep) ? keep : 30;
                    var includeTimestamp = bool.TryParse(fileSection["IncludeTimestamp"], out var timestamp) ? timestamp : true;
                    var includeLogLevel = bool.TryParse(fileSection["IncludeLogLevel"], out var level) ? level : true;
                    var includeCategory = bool.TryParse(fileSection["IncludeCategory"], out var category) ? category : true;
                    
                    System.Diagnostics.Debug.WriteLine($"File logging configuration:");
                    System.Diagnostics.Debug.WriteLine($"  FolderPath: {folderPath}");
                    System.Diagnostics.Debug.WriteLine($"  FileName: {fileName}");
                    System.Diagnostics.Debug.WriteLine($"  MaxFileSizeInMB: {maxFileSizeInMB}");
                    System.Diagnostics.Debug.WriteLine($"  MaxFilesToKeep: {maxFilesToKeep}");
                    System.Diagnostics.Debug.WriteLine($"  IncludeTimestamp: {includeTimestamp}");
                    System.Diagnostics.Debug.WriteLine($"  IncludeLogLevel: {includeLogLevel}");
                    System.Diagnostics.Debug.WriteLine($"  IncludeCategory: {includeCategory}");
                    
                    // Check if path is absolute
                    var isAbsolute = Path.IsPathRooted(folderPath);
                    System.Diagnostics.Debug.WriteLine($"  Is absolute path: {isAbsolute}");
                    
                    _fileLoggerProvider = new FileLoggerProvider(
                        folderPath, fileName, maxFileSizeInMB, maxFilesToKeep, 
                        includeTimestamp, includeLogLevel, includeCategory);
                    
                    builder.AddProvider(_fileLoggerProvider);
                    
                    // Log the file logging configuration
                    System.Diagnostics.Debug.WriteLine($"File logging configured: {folderPath}/{fileName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("File logging section not found in configuration");
                    System.Diagnostics.Debug.WriteLine("Available sections:");
                    var loggingSection = _configuration.GetSection("Logging");
                    foreach (var child in loggingSection.GetChildren())
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {child.Key}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Configuration is null, cannot configure file logging");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to configure file logging: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    private static void LoadLoggingConfiguration()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Loading logging configuration...");
            
            // First, test if we can access the file from the bundle
            try
            {
                using var testStream = FileSystem.OpenAppPackageFileAsync("logging_config.json").Result;
                System.Diagnostics.Debug.WriteLine("Successfully opened logging_config.json from bundle");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open logging_config.json from bundle: {ex.Message}");
            }
            
            var configPath = Path.Combine(FileSystem.AppDataDirectory, "logging_config.json");
            System.Diagnostics.Debug.WriteLine($"Looking for config at: {configPath}");
            
            // If config file doesn't exist in app data, copy from app bundle
            if (!File.Exists(configPath))
            {
                System.Diagnostics.Debug.WriteLine("Config file not found in app data, copying from bundle...");
                CopyLoggingConfigFromBundle(configPath);
            }
            
            if (File.Exists(configPath))
            {
                System.Diagnostics.Debug.WriteLine("Config file found, reading content...");
                var jsonContent = File.ReadAllText(configPath);
                System.Diagnostics.Debug.WriteLine($"Config content length: {jsonContent.Length}");
                System.Diagnostics.Debug.WriteLine($"Config content: {jsonContent}");
                
                // Use proper JSON configuration provider
                _configuration = new ConfigurationBuilder()
                    .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent)))
                    .Build();
                
                System.Diagnostics.Debug.WriteLine("Configuration built successfully");
                
                // Test if we can read the file logging settings
                var fileSection = _configuration.GetSection("Logging:File");
                if (fileSection.Exists())
                {
                    var folderPath = fileSection["FolderPath"];
                    System.Diagnostics.Debug.WriteLine($"File logging folder path: {folderPath}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("File section not found in configuration");
                }
                
                // Dump entire configuration for debugging
                System.Diagnostics.Debug.WriteLine("=== FULL CONFIGURATION DUMP ===");
                var loggingSection = _configuration.GetSection("Logging");
                DumpConfigurationSection(loggingSection, "Logging");
                System.Diagnostics.Debug.WriteLine("=== END CONFIGURATION DUMP ===");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Config file still not found after copy attempt");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load logging configuration: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    private static void CopyLoggingConfigFromBundle(string targetPath)
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("logging_config.json").Result;
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(targetPath, content);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to copy logging config from bundle: {ex.Message}");
        }
    }
    
    private static void DumpConfigurationSection(IConfigurationSection section, string prefix)
    {
        System.Diagnostics.Debug.WriteLine($"{prefix}:");
        foreach (var child in section.GetChildren())
        {
            if (child.Value != null)
            {
                System.Diagnostics.Debug.WriteLine($"  {prefix}.{child.Key} = {child.Value}");
            }
            else
            {
                DumpConfigurationSection(child, $"{prefix}.{child.Key}");
            }
        }
    }
    
    private static LogLevel GetMinimumLogLevel()
    {
        try
        {
            if (_configuration != null)
            {
                var logLevelStr = _configuration["Logging:LogLevel:Default"];
                if (Enum.TryParse<LogLevel>(logLevelStr, out var logLevel))
                {
                    return logLevel;
                }
            }
        }
        catch
        {
            // Ignore errors and use default
        }
        
        return LogLevel.Information;
    }
    
    public static ILogger GetLogger<T>()
    {
        if (_loggerFactory == null)
        {
            Initialize();
        }
        return _loggerFactory!.CreateLogger<T>();
    }
    
    public static ILogger GetLogger(string categoryName)
    {
        if (_loggerFactory == null)
        {
            Initialize();
        }
        return _loggerFactory!.CreateLogger(categoryName);
    }
    
    public static void LogInformation(string message)
    {
        _logger?.LogInformation(message);
    }
    
    public static void LogWarning(string message)
    {
        _logger?.LogWarning(message);
    }
    
    public static void LogError(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            _logger?.LogError(exception, message);
        }
        else
        {
            _logger?.LogError(message);
        }
    }
    
    public static void LogDebug(string message)
    {
        _logger?.LogDebug(message);
    }
    
    public static void LogCritical(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            _logger?.LogCritical(exception, message);
        }
        else
        {
            _logger?.LogCritical(message);
        }
    }
    
    public static void LogTrace(string message)
    {
        _logger?.LogTrace(message);
    }
    
    public static void Dispose()
    {
        _fileLoggerProvider?.Dispose();
        _loggerFactory?.Dispose();
    }
    
    // Method to change log level at runtime
    public static void SetLogLevel(LogLevel level)
    {
        try
        {
            Dispose();
            
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddDebug()
                    .AddConsole()
                    .SetMinimumLevel(level);
                
                // Re-add file logging if it was configured
                if (_fileLoggerProvider != null)
                {
                    builder.AddProvider(_fileLoggerProvider);
                }
            });
            
            _logger = _loggerFactory.CreateLogger("TestLLM");
            _logger.LogInformation("Log level changed to {Level}", level);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to change log level: {ex.Message}");
        }
    }
    
    // Method to get log file path
    public static string? GetLogFilePath()
    {
        try
        {
            if (_configuration != null)
            {
                var folderPath = _configuration["Logging:File:FolderPath"] ?? "logs";
                var fileName = _configuration["Logging:File:FileName"] ?? "testllm-{Date}.log";
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                fileName = fileName.Replace("{Date}", date);
                
                // Check if the path is absolute
                if (Path.IsPathRooted(folderPath))
                {
                    return Path.Combine(folderPath, fileName);
                }
                else
                {
                    return Path.Combine(FileSystem.AppDataDirectory, folderPath, fileName);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get log file path: {ex.Message}");
        }
        
        return null;
    }
    
    // Method to get all log files
    public static List<string> GetLogFiles()
    {
        var logFiles = new List<string>();
        
        try
        {
            if (_configuration != null)
            {
                var folderPath = _configuration["Logging:File:FolderPath"] ?? "logs";
                
                // Check if the path is absolute
                string fullPath;
                if (Path.IsPathRooted(folderPath))
                {
                    fullPath = folderPath;
                }
                else
                {
                    fullPath = Path.Combine(FileSystem.AppDataDirectory, folderPath);
                }
                
                if (Directory.Exists(fullPath))
                {
                    logFiles.AddRange(Directory.GetFiles(fullPath, "*.log"));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get log files: {ex.Message}");
        }
        
        return logFiles;
    }
} 