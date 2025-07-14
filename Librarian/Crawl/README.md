# Librarian Crawler System

A modular, extensible background task processing framework for Windows consisting of a Windows Service, Blazor web application, and plugin-based architecture.

## System Components

### 1. Librarian.Core
- **Description**: Core library containing the `ICrawler` interface
- **Purpose**: Provides common abstraction for all crawler implementations
- **Files**: `ICrawler.cs`

### 2. FileCrawler
- **Description**: Reference implementation of a file system crawler
- **Purpose**: Demonstrates how to implement the `ICrawler` interface
- **Functionality**: Scans directories and lists all files with metadata
- **Files**: `FileCrawler.cs`

### 3. Librarian.Service
- **Description**: Windows Service that hosts the Quartz.NET scheduler
- **Purpose**: Executes scheduled crawler tasks in the background
- **Features**:
  - Reads configuration from `appsettings.json`
  - Dynamic assembly loading using reflection
  - Concurrency control to prevent duplicate job execution
  - Robust error handling and logging with Serilog
- **Files**: `Program.cs`, `Services/CrawlerJob.cs`, `Services/CrawlerSchedulerService.cs`

### 4. Librarian.Scheduler
- **Description**: Blazor Server application for managing scheduled tasks
- **Purpose**: Provides web-based UI for configuring crawler jobs
- **Features**:
  - Syncfusion Blazor components for professional UI
  - CRUD operations for scheduled jobs
  - Dynamic crawler discovery
  - Schedule management with preset options
- **Files**: `Components/Pages/Home.razor`, `Services/ConfigurationService.cs`

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Windows OS (for the Windows Service)
- Syncfusion Blazor license (Community license available)

### Building the System
```bash
dotnet build Librarian.sln
```

### Installing the Windows Service
1. Open Command Prompt as Administrator
2. Navigate to the project directory
3. Run: `sc create "Librarian Crawler" binPath="C:\Projects\BlazorProjects\Librarian\Crawl\Librarian.Service\bin\Debug\net8.0\Librarian.Service.exe"`
4. Start the service: `sc start "Librarian Crawler"`

### Running the Scheduler UI
```bash
dotnet run --project Librarian.Scheduler
```
Access the scheduler at: http://localhost:5000

## Configuration

### Service Configuration (appsettings.json)
```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/librarian-.log" } }
    ]
  },
  "ScheduledJobs": [
    {
      "IsEnabled": true,
      "Schedule": "0 */10 * * * ?",
      "CrawlerAssembly": "FileCrawler.dll",
      "CrawlerType": "FileCrawler.FileCrawler",
      "Parameter": "C:\\Temp"
    }
  ]
}
```

### Scheduler Configuration (appsettings.json)
```json
{
  "AppSettingsPath": "C:\\Program Files\\Librarian\\appsettings.json",
  "CrawlersPath": "C:\\Program Files\\Librarian"
}
```

## Creating Custom Crawlers

1. Create a new C# Class Library project
2. Add reference to `Librarian.Core`
3. Implement the `ICrawler` interface:

```csharp
using Librarian.Core;

public class MyCrawler : ICrawler
{
    public void Run(object parameter)
    {
        // Your crawler logic here
    }
}
```

4. Build the project and place the DLL in the crawlers directory
5. Use the Scheduler UI to configure the new crawler

## Schedule Expressions

The system uses Quartz.NET cron expressions:
- `0 */10 * * * ?` - Every 10 minutes
- `0 0 * * * ?` - Every hour
- `0 0 0 * * ?` - Daily at midnight
- `0 0 0 ? * SUN` - Weekly on Sundays
- `0 0 0 1 * ?` - Monthly on the 1st

## Architecture Features

### Concurrency Control
- Prevents multiple instances of the same crawler type with identical parameters
- Uses `ConcurrentDictionary` for thread-safe job tracking

### Error Handling
- Individual job failures don't crash the service
- Comprehensive logging of all operations
- Graceful handling of assembly loading errors

### Extensibility
- Plugin-based architecture for easy crawler development
- Dynamic assembly discovery and loading
- Configurable through web interface

## Directory Structure
```
Librarian.Crawl/
├── Librarian.Core/          # Core interface library
├── FileCrawler/             # Reference crawler implementation
├── Librarian.Service/       # Windows Service
├── Librarian.Scheduler/     # Blazor web application
├── Librarian.sln           # Solution file
└── README.md               # This file
```

## Support

For issues and feature requests, please refer to the development team or create issues in the project repository.