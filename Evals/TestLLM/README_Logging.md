# Logging System for TestLLM

This document explains the logging system implemented in the TestLLM application.

## Overview

The TestLLM application uses Microsoft.Extensions.Logging to provide comprehensive logging capabilities. The logging system is designed to help with debugging, monitoring, and troubleshooting the application.

## Features

- **Multiple Log Providers**: Console, Debug, and File output
- **Configurable Log Levels**: Can be adjusted via configuration file
- **Structured Logging**: Uses structured logging with parameters
- **Category-based Logging**: Different log levels for different components
- **Runtime Log Level Changes**: Can change log levels without restarting
- **File Logging**: Automatic log file rotation and cleanup
- **Customizable Log Format**: Configurable timestamp, log level, and category inclusion

## Log Levels

The application supports the following log levels (from lowest to highest priority):

- **Trace**: Detailed diagnostic information
- **Debug**: Diagnostic information for debugging
- **Information**: General information about application flow
- **Warning**: Warning messages for potentially problematic situations
- **Error**: Error messages for error conditions
- **Critical**: Critical error messages for severe problems

## Configuration

### Logging Configuration File

The application uses `logging_config.json` to configure logging behavior. This file is located in the app's data directory and can be modified to adjust logging settings.

Example configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "TestLLM": "Debug",
      "TestLLM.LLMConfigService": "Debug",
      "TestLLM.MainPage": "Information"
    },
    "Console": {
      "LogLevel": {
        "Default": "Information",
        "TestLLM": "Debug"
      }
    },
    "Debug": {
      "LogLevel": {
        "Default": "Debug",
        "TestLLM": "Debug"
      }
    },
    "File": {
      "LogLevel": {
        "Default": "Information",
        "TestLLM": "Debug"
      },
      "FolderPath": "logs",
      "FileName": "testllm-{Date}.log",
      "MaxFileSizeInMB": 10,
      "MaxFilesToKeep": 30,
      "IncludeTimestamp": true,
      "IncludeLogLevel": true,
      "IncludeCategory": true
    }
  }
}
```

### Configuration Options

#### General Logging
- **Default**: Default log level for all categories
- **Microsoft**: Log level for Microsoft framework components
- **TestLLM**: Log level for all TestLLM components
- **TestLLM.LLMConfigService**: Specific log level for configuration service
- **TestLLM.MainPage**: Specific log level for main page

#### File Logging
- **FolderPath**: Directory where log files are stored (relative to app data directory)
- **FileName**: Log file name pattern (supports {Date} placeholder)
- **MaxFileSizeInMB**: Maximum file size before rotation (default: 10MB)
- **MaxFilesToKeep**: Maximum number of log files to keep (default: 30)
- **IncludeTimestamp**: Whether to include timestamp in log entries (default: true)
- **IncludeLogLevel**: Whether to include log level in log entries (default: true)
- **IncludeCategory**: Whether to include category name in log entries (default: true)

## File Logging

### Features

- **Automatic Rotation**: Log files are automatically rotated when they reach the maximum size
- **Date-based Naming**: Log files can include the current date in the filename
- **Automatic Cleanup**: Old log files are automatically deleted based on the retention policy
- **Thread-safe**: File writing is thread-safe with proper locking
- **Error Handling**: Graceful handling of file system errors

### Log File Format

Example log entry:
```
[2024-01-15 14:30:25.123] [Information] [TestLLM.MainPage] Application started
[2024-01-15 14:30:25.456] [Debug] [TestLLM.LLMConfigService] Loading LLMs from config
[2024-01-15 14:30:25.789] [Error] [TestLLM.MainPage] Failed to connect to server Exception: Connection refused
```

### File Locations

- **Default**: `{AppDataDirectory}/logs/testllm-{Date}.log`
- **Custom**: Can be configured via `FolderPath` and `FileName` settings

### Important Note for macOS

On macOS, applications run in a sandboxed environment and can only write to their own app data directory. The `FolderPath` setting should be a relative path (e.g., "logs") that will be resolved relative to the app's data directory. Absolute paths outside the app's container will result in permission denied errors.

**App Data Directory Locations:**
- **macOS**: `/Users/{username}/Library/Containers/{app-id}/Data/Library/`
- **iOS**: App's documents directory
- **Android**: App's internal storage
- **Windows**: App's local data directory

**Example:**
- If `FolderPath` is set to "logs", log files will be stored at:
  - macOS: `/Users/{username}/Library/Containers/com.companyname.testllm/Data/Library/logs/`
  - Other platforms: `{AppDataDirectory}/logs/`

### File Rotation

When a log file reaches the maximum size:
1. Current file is renamed with timestamp: `testllm-2024-01-15-143025.log`
2. New log file is created: `testllm-2024-01-15.log`
3. Old files beyond the retention limit are deleted

## Usage

### Basic Logging

```csharp
// Get a logger for a specific class
private static readonly ILogger _logger = LoggingService.GetLogger<MyClass>();

// Log messages
_logger.LogInformation("Application started");
_logger.LogDebug("Processing request: {RequestId}", requestId);
_logger.LogWarning("Configuration file not found");
_logger.LogError(ex, "Failed to connect to server");
```

### Static Logging Methods

```csharp
// Use static methods for quick logging
LoggingService.LogInformation("Simple info message");
LoggingService.LogWarning("Warning message");
LoggingService.LogError("Error message", exception);
LoggingService.LogDebug("Debug message");
```

### Runtime Log Level Changes

```csharp
// Change log level at runtime
LoggingService.SetLogLevel(LogLevel.Debug);
```

### File Logging Utilities

```csharp
// Get current log file path
string? logFilePath = LoggingService.GetLogFilePath();

// Get all log files
List<string> logFiles = LoggingService.GetLogFiles();
```

## Log Categories

The application uses the following log categories:

- **TestLLM**: General application logs
- **TestLLM.LLMConfigService**: Configuration service logs
- **TestLLM.MainPage**: Main page UI logs
- **Microsoft**: Framework logs (filtered by default)

## Log Output

### Debug Output

In debug builds, logs are output to the debug console and can be viewed in:
- Visual Studio Output window
- Visual Studio Code Debug Console
- Xcode Console (for iOS/macOS)

### Console Output

In release builds, logs are output to the console and can be viewed in:
- Terminal/Command Prompt
- Application logs on mobile devices

### File Output

Log files are stored in the configured directory and can be viewed with any text editor. The files are automatically rotated and cleaned up based on the configuration.

## Best Practices

### When to Use Each Log Level

- **Trace**: Very detailed diagnostic information (rarely used)
- **Debug**: Information useful for debugging during development
- **Information**: General flow information, startup, shutdown
- **Warning**: Unexpected situations that don't stop execution
- **Error**: Error conditions that affect functionality
- **Critical**: Severe errors that may cause application failure

### Structured Logging

Use structured logging with parameters instead of string concatenation:

```csharp
// Good
_logger.LogInformation("User {UserId} logged in from {IPAddress}", userId, ipAddress);

// Avoid
_logger.LogInformation($"User {userId} logged in from {ipAddress}");
```

### Exception Logging

Always include exceptions when logging errors:

```csharp
try
{
    // Some operation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to perform operation");
}
```

### File Logging Considerations

- **Performance**: File logging has minimal impact on performance
- **Storage**: Monitor log file sizes and adjust retention settings as needed
- **Security**: Log files may contain sensitive information, ensure proper access controls
- **Backup**: Consider backing up important log files

## Troubleshooting

### Common Issues

1. **No logs appearing**: Check if the log level is set too high
2. **Too many logs**: Increase the log level to reduce verbosity
3. **Configuration not loading**: Check if `logging_config.json` exists in app data directory
4. **File logging not working**: Check if the log directory is writable
5. **Large log files**: Adjust `MaxFileSizeInMB` and `MaxFilesToKeep` settings

### Debugging Logging Issues

If logging itself isn't working, check the debug output for initialization errors. The logging service has fallback mechanisms to ensure basic logging always works.

### File Logging Issues

- **Permission errors**: Ensure the app has write permissions to the log directory
- **Disk space**: Monitor available disk space for log files
- **File rotation**: Check if file rotation is working correctly

## File Locations

- **Development**: `TestLLM/logging_config.json` (included in app bundle)
- **Runtime**: App data directory (copied from bundle on first run)
- **Log Files**: `{AppDataDirectory}/{FolderPath}/{FileName}` (configurable)

## Performance Considerations

- Debug and Trace logs are automatically filtered out in release builds
- Structured logging parameters are only evaluated if the log level allows the message to be logged
- Logging configuration is loaded once at startup for performance
- File logging uses buffered writes and proper locking for performance
- Log file rotation and cleanup happen asynchronously to avoid blocking 