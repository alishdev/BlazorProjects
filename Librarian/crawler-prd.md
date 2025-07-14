# Product Requirements Document: Librarian Crawler System

**Version:** 1.0
**Date:** October 26, 2023
**Author:** [Your Name/Company Name]

### 1. Introduction

The Librarian Crawler System is a modular, extensible background task processing framework for Windows. It consists of a core Windows Service responsible for running scheduled jobs, a Blazor web application for configuring those jobs, and a plugin-based architecture for defining the jobs themselves.

The primary goal is to create a reliable system that can execute custom tasks (crawlers) on a recurring schedule, with the configuration being managed centrally through a user-friendly web interface.

### 2. System Architecture Overview

The system is composed of three main components:

1.  **Librarian Crawler (Windows Service):** The core engine. This service runs in the background on a Windows machine. It hosts a scheduler that reads its configuration from `appsettings.json`, dynamically loads the appropriate crawler DLLs, and executes them according to the defined schedule.
2.  **Librarian Crawler Scheduler (Blazor Application):** The management UI. This is a cross-platform web application that provides an interface for administrators to add, edit, and delete scheduled tasks. Its sole responsibility is to modify the `appsettings.json` file used by the Librarian Crawler service.
3.  **Crawler Implementations (C# Class Libraries):** These are individual DLLs that contain the business logic for a specific task. Each crawler must implement the `ICrawler` interface. The `File Crawler` will be the first reference implementation.

### 3. Functional Requirements

#### 3.1. Common Components & Interfaces

##### 3.1.1. ICrawler Interface
A dedicated C# Class Library project (e.g., `Librarian.Core.dll`) shall be created to house the common interface. This ensures that both the Service and the Crawler implementations depend on a common abstraction.

*   **Interface Definition:**
    ```csharp
    namespace Librarian.Core
    {
        public interface ICrawler
        {
            /// <summary>
            /// The main execution method for the crawler.
            /// </summary>
            /// <param name="parameter">A configuration object passed from the scheduler.</param>
            void Run(object parameter);
        }
    }
    ```

#### 3.2. Component A: Librarian Crawler (Windows Service)

This is a .NET Windows Service project.

##### 3.2.1. Service Behavior
*   The service must be installable as a standard Windows Service.
*   The service startup type must be configured to "Automatic" so that it starts when Windows boots.
*   The service must be resilient. An unhandled exception in one crawler instance should not crash the entire service. Each job execution must be wrapped in a `try-catch` block with robust logging.

##### 3.2.2. Configuration (`appsettings.json`)
*   The service must read its configuration from an `appsettings.json` file located in its execution directory.
*   The file should contain a section for configuring the scheduled jobs. The structure should be an array of job objects.

*   **Example `appsettings.json`:**
    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.Hosting.Lifetime": "Information"
        }
      },
      "ScheduledJobs": [
        {
          "IsEnabled": true,
          "Schedule": "0 */10 * * * ?", // Cron expression for every 10 minutes
          "CrawlerAssembly": "FileCrawler.dll", // The assembly containing the type
          "CrawlerType": "FileCrawler.FileCrawler", // The fully qualified class name
          "Parameter": "C:\\Temp"
        },
        {
          "IsEnabled": true,
          "Schedule": "0 0 2 * * ?", // Cron expression for 2 AM every day
          "CrawlerAssembly": "AnotherCrawler.dll",
          "CrawlerType": "AnotherCrawler.SomeOtherTask",
          "Parameter": { // Parameter can be a complex object
             "ApiUrl": "https://api.example.com",
             "TimeoutSeconds": 30
          }
        }
      ]
    }
    ```

##### 3.2.3. Scheduler Engine
*   The service must host a scheduler. A robust, well-maintained library such as **Quartz.NET** is highly recommended for this purpose.
*   On startup, the scheduler will read the `ScheduledJobs` array from `appsettings.json`.
*   For each enabled job, it will schedule a task to run based on its `Schedule` (Cron expression).
*   The scheduler must dynamically load the assembly specified in `CrawlerAssembly` and instantiate the class specified in `CrawlerType` using reflection.

##### 3.2.4. Concurrency Control
*   The scheduler **must** prevent more than one instance of the same `CrawlerType` from running concurrently with the exact same `Parameter`.
*   **Mechanism:** Before executing a job, the scheduler should check a central registry (e.g., a `ConcurrentDictionary<string, object>`) to see if a job with an identical signature (`CrawlerType` + `Parameter`) is already running. If it is, the new execution should be skipped and a warning logged.

#### 3.3. Component B: Librarian Crawler Scheduler (Blazor Application)

This is a Blazor Server application to ensure it can run on Windows/Mac and reliably access a file path on the host machine.

##### 3.3.1. Platform & UI
*   The application must be built using **Blazor Server**.
*   The UI must be built using **Syncfusion Blazor components**.
*   The application must be runnable on Windows and macOS via the .NET runtime.

##### 3.3.2. Core Functionality
*   The application's primary function is to provide a graphical interface for editing the `appsettings.json` file of the Librarian Crawler service.
*   The path to the `appsettings.json` file must be configurable within the Blazor app's own settings.

##### 3.3.3. User Interface - Job Management
*   **Main View:**
    *   Display a list of all scheduled jobs from `appsettings.json` in a **Syncfusion DataGrid**.
    *   Columns should include: Schedule (human-readable), Crawler Module, Parameter, and an Enabled/Disabled status toggle.
    *   The grid must have buttons for "Add New Job", "Edit", and "Delete".

*   **Add/Edit Job Dialog:**
    *   This will be a **Syncfusion Dialog** or modal form.
    *   **Schedule/Frequency:** A **Syncfusion DropdownList** with pre-configured, user-friendly options (e.g., "Every 5 Minutes", "Every Hour", "Daily at Midnight"). These options will map to the corresponding Cron expressions. An "Advanced" option should show a text box for entering a raw Cron expression.
    *   **Crawler Module:** A **Syncfusion DropdownList** that is dynamically populated. The Blazor app will scan a designated "crawlers" directory (where the crawler DLLs are placed) and use reflection to find all public classes that implement `ICrawler`. The dropdown will display the class names (e.g., `FileCrawler`).
    *   **Parameter:** A **Syncfusion TextBox** for the user to enter the parameter. For the `FileCrawler`, this would be a folder path like `C:\Temp`.
    *   **Enabled:** A **Syncfusion Checkbox** or Toggle Switch.

*   **Saving:** Upon saving, the application will programmatically update the `ScheduledJobs` section of the `appsettings.json` file and save it to disk.

#### 3.4. Component C: File Crawler (C# Class Library)

This is a C# Class Library (.dll) project that serves as the first implementation of a crawler.

*   **Project Name:** `FileCrawler`
*   **Dependencies:** Must reference the `Librarian.Core.dll` project.
*   **Implementation:**
    *   Create a public class named `FileCrawler` that implements the `ICrawler` interface.
    *   The `Run(object parameter)` method will:
        1.  Validate that the `parameter` is not null and is a string.
        2.  Cast the `parameter` to a string, representing the directory path.
        3.  Check if the directory exists.
        4.  *Proof-of-Concept Logic:* Iterate through all files in the given directory and its subdirectories, writing each file's full path to a log file or the console. This demonstrates that the crawler is executing correctly.
        5.  Include error handling (e.g., for "Directory not found" or "Access denied" exceptions).

### 4. Non-Functional Requirements

*   **Reliability:** The Windows service must run 24/7 without crashing. Individual crawler failures must be isolated and logged.
*   **Usability:** The Blazor Scheduler application must be intuitive for a non-developer to configure, schedule, and manage jobs.
*   **Maintainability:** The code for all components should be clean, well-commented, and follow standard C#/.NET best practices, including the use of Dependency Injection in both the service and the Blazor app.
*   **Logging:** The Windows Service must implement robust logging (e.g., using Serilog or NLog) to record scheduler activity, job executions, successes, and failures.

### 5. Technical Stack

*   **Language:** C#
*   **Framework:** .NET 8 (or latest stable)
*   **Projects:**
    *   Windows Service
    *   Blazor Server Application
    *   C# Class Library (for `ICrawler` interface)
    *   C# Class Library (for `FileCrawler` implementation)
*   **Key Libraries:**
    *   **Syncfusion for Blazor:** For all UI components in the Scheduler app.
    *   **Quartz.NET:** Recommended for the scheduling engine in the Windows Service.
    *   **Microsoft.Extensions.Hosting:** For building the Windows Service.