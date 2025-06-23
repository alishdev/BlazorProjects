using Microsoft.Extensions.Logging;

namespace TestLLM;

public static class LoggingService
{
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;
    
    public static void Initialize()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddDebug()
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug);
        });
        
        _logger = _loggerFactory.CreateLogger("TestLLM");
        _logger.LogInformation("Logging service initialized");
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
    
    public static void Dispose()
    {
        _loggerFactory?.Dispose();
    }
} 