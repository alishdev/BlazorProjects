# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a modular .NET 8.0/9.0 background task processing framework for Windows consisting of:
- A Windows Service (`Librarian.Service`) that hosts Quartz.NET scheduler
- A Windows Forms desktop application (`Librarian.Scheduler`) with Blazor WebView for managing scheduled tasks
- A cross-platform MAUI application (`Librarian.FileViewer`) for viewing file hierarchies
- A plugin-based architecture using the `ICrawler` interface (`Librarian.Core`)
- Reference implementation (`FileCrawler`) demonstrating the crawler pattern

## Development Commands

### Building the Solution
```bash
dotnet build Librarian.sln
```

### Running the Scheduler Desktop App
```bash
dotnet run --project Librarian.Scheduler
```

### Running the File Viewer MAUI App
```bash
dotnet run --project Librarian.FileViewer --framework net9.0-windows10.0.19041.0
```

### Testing the System
```powershell
.\test-system.ps1
```

### Installing the Windows Service
```cmd
sc create "Librarian Crawler" binPath="C:\Projects\BlazorProjects\Librarian\Crawl\Librarian.Service\bin\Debug\net8.0\Librarian.Service.exe"
sc start "Librarian Crawler"
```

## Architecture

### Core Components
- **Librarian.Core**: Contains `ICrawler` interface - the contract all crawlers must implement
- **Librarian.Service**: Windows Service using Quartz.NET for scheduling, dynamic assembly loading, and concurrent execution control
- **Librarian.Scheduler**: Windows Forms application with Blazor WebView using Syncfusion components for job management
- **Librarian.FileViewer**: Cross-platform MAUI application with Blazor WebView for viewing file hierarchies from `file_hierarchy.json`
- **FileCrawler**: Reference implementation showing file system crawling

### Key Design Patterns
- **Plugin Architecture**: Crawlers are loaded dynamically via reflection from assemblies
- **Dependency Injection**: All projects use Microsoft.Extensions.DependencyInjection
- **Concurrency Control**: Service prevents duplicate job execution using ConcurrentDictionary
- **Configuration-driven**: Jobs configured via JSON, manageable through web UI

### Data Flow
1. Scheduler UI manages job configurations in `appsettings.json`
2. Service reads configuration and schedules jobs with Quartz.NET
3. Service dynamically loads crawler assemblies and executes them
4. All operations logged with Serilog to console and rolling files

## Configuration

### Service Configuration (Librarian.Service/appsettings.json)
- `Serilog`: Logging configuration with console and file sinks
- `ScheduledJobs`: Array of job configurations with cron schedules

### Scheduler Configuration
- Configuration is stored in Windows AppData directory
- Windows: `%APPDATA%\Librarian\appsettings.json`
- Crawler assemblies are stored in the same directory structure

## Creating New Crawlers

1. Create new Class Library targeting .NET 8.0
2. Reference `Librarian.Core` project
3. Implement `ICrawler` interface with `Run(object parameter)` method
4. Build and place DLL in crawlers directory
5. Configure through Scheduler UI

## Testing

Use `test-system.ps1` which:
- Builds all projects
- Tests FileCrawler functionality
- Validates service and scheduler compilation
- Provides deployment instructions

## Technology Stack

- **.NET 8.0/9.0**: Primary framework
- **Quartz.NET**: Job scheduling
- **Serilog**: Logging
- **Syncfusion Blazor**: UI components
- **Microsoft.Extensions.Hosting**: Windows Service hosting
- **Windows Forms**: Desktop application framework
- **Blazor WebView**: Blazor components in Windows Forms
- **.NET MAUI**: Cross-platform application framework
- **Blazor WebView MAUI**: Blazor components in MAUI applications

## File Viewer Application

The `Librarian.FileViewer` is a cross-platform MAUI application that provides a two-panel interface for viewing file hierarchies:

### Features
- **Left Panel**: File hierarchy tree view displaying structure from `file_hierarchy.json`
- **Right Panel**: File content preview with support for:
  - Text files (code, JSON, markdown)
  - Image files with preview
  - Binary files with metadata display
- **Responsive Design**: Works on Windows and macOS
- **Syncfusion Components**: Modern UI with Syncfusion Blazor components

### Configuration
- Reads `file_hierarchy.json` from `C:\Projects\BlazorProjects\Librarian\Crawl\`
- Automatically parses nested JSON structures
- Supports both simple and complex file hierarchy formats
- Cached hierarchy data for performance

### Key Components
- **FileViewer.razor**: Main page with two-panel layout
- **FileTreeNode.razor**: Recursive tree node component
- **FileHierarchyService**: Service for reading and parsing JSON hierarchy
- **FileContentService**: Service for reading and displaying file contents