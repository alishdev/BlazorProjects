using System.Collections.ObjectModel;
using Python.Runtime;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;

namespace TestLLM;

// Fallback LLM utility class that doesn't require Python
public class FallbackLLMUtil
{
    public string AskLLM(string llmName, string prompt)
    {
        return $"Response from {llmName}:\n\nPrompt: {prompt}\n\nThis is a fallback response from {llmName}. Python integration is not available.";
    }
}

public partial class MainPage : ContentPage
{
    private static readonly ILogger _logger = LoggingService.GetLogger<MainPage>();
    private ObservableCollection<LLM> _llmList;
    private Dictionary<string, Button> _tabButtons;
    private Dictionary<string, CheckBox> _checkBoxes;
    private string _currentSelectedTab = "Settings";

    public MainPage()
    {
        _logger.LogInformation("Initializing MainPage");
        InitializeComponent();
        
        // Initialize collections
        _logger.LogDebug("Loading LLMs from config");
        _llmList = new ObservableCollection<LLM>(LLMConfigService.LoadLLMsFromConfig());
        _logger.LogInformation("Loaded {Count} LLMs from config", _llmList.Count);
        
        _tabButtons = new Dictionary<string, Button>();
        _checkBoxes = new Dictionary<string, CheckBox>();
        
        // Initialize UI
        InitializeTabs();
        InitializeSettingsCheckboxes();
        
        // Set initial state
        ShowSettingsTab();
        
        // Test file logging
        LogFileInformation();
        
        _logger.LogInformation("MainPage initialization completed");
    }

    private void InitializeTabs()
    {
        _logger.LogDebug("Initializing tabs for {Count} LLMs", _llmList.Count);
        
        // Add Settings tab button to dictionary
        _tabButtons["Settings"] = SettingsTab;
        
        // Create tab buttons for each LLM
        foreach (var llm in _llmList)
        {
            string llm_model = $"{llm.Name}:{llm.DefaultModel}";
            var tabButton = new Button
            {
                Text = $"🤖 {llm_model}",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                CornerRadius = 12,
                Padding = new Thickness(16, 12),
                FontSize = 21,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 0, 0, 8),
                HorizontalOptions = LayoutOptions.Start
            };
            
            tabButton.Clicked += OnTabClicked;
            _tabButtons[llm_model] = tabButton;
            TabHeaders.Children.Add(tabButton);
            _logger.LogDebug("Created tab for LLM: {Name}", llm_model);
        }
    }

    private void InitializeSettingsCheckboxes()
    {
        _logger.LogDebug("Initializing settings checkboxes for {Count} LLMs", _llmList.Count);
        
        foreach (var llm in _llmList)
        {
            var checkBox = new CheckBox
            {
                IsChecked = true,
                Color = Color.FromArgb("#3B82F6")
            };
            
            var label = new Label
            {
                Text = llm.Name,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16,
                TextColor = Color.FromArgb("#1E293B"),
                FontAttributes = FontAttributes.Bold
            };
            
            var description = new Label
            {
                Text = llm.Description,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 14,
                TextColor = Color.FromArgb("#64748B")
            };
            
            var textStack = new VerticalStackLayout
            {
                Children = { label, description },
                Spacing = 2
            };
            
            var horizontalStack = new HorizontalStackLayout
            {
                Children = { checkBox, textStack },
                Spacing = 16,
                VerticalOptions = LayoutOptions.Center
            };
            
            var container = new Border
            {
                Content = horizontalStack,
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                Padding = new Thickness(16, 12),
                Margin = new Thickness(0, 0, 0, 8)
            };
            
            checkBox.CheckedChanged += (sender, e) => OnCheckBoxChanged(llm.Name, e.Value);
            
            _checkBoxes[llm.Name] = checkBox;
            SettingsCheckboxes.Children.Add(container);
            _logger.LogDebug("Created checkbox for LLM: {Name}", llm.Name);
        }
    }

    private void OnTabClicked(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            string tabName = button.Text.Replace("⚙️ ", "").Replace("🤖 ", "");
            _currentSelectedTab = tabName;
            _logger.LogDebug("Tab clicked: {TabName}", tabName);
            
            // Update button colors
            foreach (var tab in _tabButtons.Values)
            {
                tab.BackgroundColor = Colors.Transparent;
            }
            button.BackgroundColor = Color.FromArgb("#3B82F6");
            
            // Show appropriate content
            if (tabName == "Settings")
            {
                ShowSettingsTab();
            }
            else
            {
                ShowLLMTab(tabName);
            }
        }
    }

    private void ShowSettingsTab()
    {
        _logger.LogDebug("Showing settings tab");
        
        // Make bottom panel cover entire right panel
        TopRow.Height = new GridLength(0);
        BottomRow.Height = new GridLength(1, GridUnitType.Star);
        
        TopPanel.IsVisible = false;
        SettingsContent.IsVisible = true;
        ResultContent.IsVisible = false;
    }

    private void ShowLLMTab(string llmName)
    {
        _logger.LogDebug("Showing LLM tab: {LLMName}", llmName);
        
        // Restore normal layout
        TopRow.Height = new GridLength(25, GridUnitType.Star);
        BottomRow.Height = new GridLength(75, GridUnitType.Star);
        
        TopPanel.IsVisible = true;
        SettingsContent.IsVisible = false;
        ResultContent.IsVisible = true;
        lResult.Text = $"Selected LLM: {llmName}";
    }

    private void OnCheckBoxChanged(string llmName, bool isChecked)
    {
        _logger.LogDebug("Checkbox changed for {LLMName}: {IsChecked}", llmName, isChecked);
        
        if (_tabButtons.ContainsKey(llmName))
        {
            var tabButton = _tabButtons[llmName];
            
            if (isChecked)
            {
                // Show the tab
                if (!TabHeaders.Children.Contains(tabButton))
                {
                    TabHeaders.Children.Add(tabButton);
                    _logger.LogDebug("Added tab for {LLMName}", llmName);
                }
            }
            else
            {
                // Hide the tab
                if (TabHeaders.Children.Contains(tabButton))
                {
                    TabHeaders.Children.Remove(tabButton);
                    _logger.LogDebug("Removed tab for {LLMName}", llmName);
                }
                
                // If this tab was selected, switch to Settings
                if (_currentSelectedTab == llmName)
                {
                    _currentSelectedTab = "Settings";
                    SettingsTab.BackgroundColor = Colors.White;
                    ShowSettingsTab();
                    _logger.LogDebug("Switched to Settings tab because {LLMName} was deselected", llmName);
                }
            }
        }
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        string question = QuestionEditor.Text?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(question))
        {
            lResult.Text = "Please enter a question first.";
            _logger.LogWarning("Submit clicked with empty question");
            return;
        }
        
        if (_currentSelectedTab == "Settings")
        {
            lResult.Text = "Please select an LLM tab to submit your question.";
            _logger.LogWarning("Submit clicked while on Settings tab");
            return;
        }
        
        _logger.LogInformation("Submitting question to {LLM}: {Question}", _currentSelectedTab, question);
        lResult.Text = await AskLLM(_currentSelectedTab, question);
        
        QuestionEditor.Text = "";
    }

    private async Task<string> AskLLM(string llm, string prompt)
    {
        try
        {
            // Find the LLM configuration
            var llmConfig = _llmList.FirstOrDefault(l => l.Name == llm);
            var model = llmConfig?.DefaultModel ?? "default";
            var apiKey = llmConfig?.ApiKey ?? llm.ToLower();
            
            _logger.LogInformation("Asking LLM: {LLM}, Model: {Model}, API Key: {ApiKey}", llm, model, apiKey);
            
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    llm = apiKey,
                    prompt = prompt,
                    model = model
                };
                
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogDebug("Sending request to server: {Json}", json);
                
                var response = await client.PostAsync("http://localhost:8000/askllm", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Received successful response from server");
                    _logger.LogDebug("Response: {Response}", responseText);
                    return responseText;
                }
                else
                {
                    var errorMessage = $"Error: HTTP {response.StatusCode} - {response.ReasonPhrase}";
                    _logger.LogError("Server returned error: {StatusCode} - {Reason}", response.StatusCode, response.ReasonPhrase);
                    return errorMessage;
                }
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error connecting to server: {ex.Message}";
            _logger.LogError(ex, "Error connecting to server: {Message}", ex.Message);
            return errorMessage;
        }
    }
    
    private void RefreshLLMListFromConfig()
    {
        _logger.LogInformation("Refreshing LLM list from config");
        
        var newLLMs = LLMConfigService.LoadLLMsFromConfig();
        
        // Clear existing collections
        _llmList.Clear();
        _tabButtons.Clear();
        _checkBoxes.Clear();
        
        // Clear UI elements
        TabHeaders.Children.Clear();
        SettingsCheckboxes.Children.Clear();
        
        // Add new LLMs
        foreach (var llm in newLLMs)
        {
            _llmList.Add(llm);
        }
        
        // Reinitialize UI
        InitializeTabs();
        InitializeSettingsCheckboxes();
        
        // Reset to Settings tab
        _currentSelectedTab = "Settings";
        ShowSettingsTab();
        
        _logger.LogInformation("LLM list refreshed with {Count} LLMs", _llmList.Count);
    }
    
    // Method to demonstrate file logging functionality
    private void LogFileInformation()
    {
        try
        {
            var logFilePath = LoggingService.GetLogFilePath();
            var logFiles = LoggingService.GetLogFiles();
            
            _logger.LogInformation("Current log file: {LogFilePath}", logFilePath ?? "Not configured");
            _logger.LogInformation("Total log files found: {Count}", logFiles.Count);
            
            // Log app data directory for debugging
            _logger.LogDebug("App data directory: {AppDataDirectory}", FileSystem.AppDataDirectory);
            
            // Check if log directory exists
            if (logFilePath != null)
            {
                var logDirectory = Path.GetDirectoryName(logFilePath);
                var directoryExists = Directory.Exists(logDirectory);
                _logger.LogDebug("Log directory: {LogDirectory}, Exists: {Exists}", logDirectory, directoryExists);
                
                if (directoryExists)
                {
                    var directoryInfo = new DirectoryInfo(logDirectory!);
                    _logger.LogDebug("Directory info - FullName: {FullName}, Attributes: {Attributes}", 
                        directoryInfo.FullName, directoryInfo.Attributes);
                }
                
                // Test direct file writing
                TestDirectFileWriting(logDirectory!, "test-direct-write.log");
            }
            
            foreach (var file in logFiles.Take(5)) // Show first 5 files
            {
                var fileInfo = new FileInfo(file);
                _logger.LogDebug("Log file: {FileName}, Size: {Size} bytes, Modified: {Modified}", 
                    fileInfo.Name, fileInfo.Length, fileInfo.LastWriteTime);
            }
            
            // Log some test messages to demonstrate file logging
            _logger.LogTrace("This is a trace message");
            _logger.LogDebug("This is a debug message");
            _logger.LogInformation("This is an information message");
            _logger.LogWarning("This is a warning message");
            _logger.LogError("This is an error message");
            
            _logger.LogInformation("File logging demonstration completed");
            
            // Check if the test log file was created
            if (logFilePath != null && File.Exists(logFilePath))
            {
                var fileInfo = new FileInfo(logFilePath);
                _logger.LogInformation("Test log file created successfully: {FileName}, Size: {Size} bytes", 
                    fileInfo.Name, fileInfo.Length);
            }
            else
            {
                _logger.LogWarning("Test log file was not created. Path: {LogFilePath}", logFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error demonstrating file logging");
        }
    }
    
    // Test direct file writing to check permissions
    private void TestDirectFileWriting(string directory, string fileName)
    {
        try
        {
            var testFilePath = Path.Combine(directory, fileName);
            var testContent = $"Test file written at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\n";
            
            System.Diagnostics.Debug.WriteLine($"Testing direct file write to: {testFilePath}");
            
            File.WriteAllText(testFilePath, testContent);
            
            if (File.Exists(testFilePath))
            {
                var fileInfo = new FileInfo(testFilePath);
                System.Diagnostics.Debug.WriteLine($"Direct file write successful: {fileInfo.Name}, Size: {fileInfo.Length} bytes");
                _logger.LogInformation("Direct file write test successful: {FileName}", fileInfo.Name);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Direct file write failed - file does not exist after write");
                _logger.LogWarning("Direct file write test failed - file not created");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Direct file write test failed: {ex.Message}");
            _logger.LogError(ex, "Direct file write test failed");
        }
    }
}