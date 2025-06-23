using System.Collections.ObjectModel;
using Python.Runtime;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq;

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
    private ObservableCollection<LLM> _llmList;
    private Dictionary<string, Button> _tabButtons;
    private Dictionary<string, CheckBox> _checkBoxes;
    private string _currentSelectedTab = "Settings";

    public MainPage()
    {
        InitializeComponent();
        
        // Initialize collections
        _llmList = new ObservableCollection<LLM>(LLMConfigService.LoadLLMsFromConfig());
        
        _tabButtons = new Dictionary<string, Button>();
        _checkBoxes = new Dictionary<string, CheckBox>();
        
        // Initialize UI
        InitializeTabs();
        InitializeSettingsCheckboxes();
        
        // Set initial state
        ShowSettingsTab();
    }

    private void InitializeTabs()
    {
        // Add Settings tab button to dictionary
        _tabButtons["Settings"] = SettingsTab;
        
        // Create tab buttons for each LLM
        foreach (var llm in _llmList)
        {
            var tabButton = new Button
            {
                Text = $"🤖 {llm.Name}",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                CornerRadius = 12,
                Padding = new Thickness(16, 12),
                FontSize = 21,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            
            tabButton.Clicked += OnTabClicked;
            _tabButtons[llm.Name] = tabButton;
            TabHeaders.Children.Add(tabButton);
        }
    }

    private void InitializeSettingsCheckboxes()
    {
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
        }
    }

    private void OnTabClicked(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            string tabName = button.Text.Replace("⚙️ ", "").Replace("🤖 ", "");
            _currentSelectedTab = tabName;
            
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
        // Make bottom panel cover entire right panel
        TopRow.Height = new GridLength(0);
        BottomRow.Height = new GridLength(1, GridUnitType.Star);
        
        TopPanel.IsVisible = false;
        SettingsContent.IsVisible = true;
        ResultContent.IsVisible = false;
    }

    private void ShowLLMTab(string llmName)
    {
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
        if (_tabButtons.ContainsKey(llmName))
        {
            var tabButton = _tabButtons[llmName];
            
            if (isChecked)
            {
                // Show the tab
                if (!TabHeaders.Children.Contains(tabButton))
                {
                    TabHeaders.Children.Add(tabButton);
                }
            }
            else
            {
                // Hide the tab
                if (TabHeaders.Children.Contains(tabButton))
                {
                    TabHeaders.Children.Remove(tabButton);
                }
                
                // If this tab was selected, switch to Settings
                if (_currentSelectedTab == llmName)
                {
                    _currentSelectedTab = "Settings";
                    SettingsTab.BackgroundColor = Colors.White;
                    ShowSettingsTab();
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
            return;
        }
        
        if (_currentSelectedTab == "Settings")
        {
            lResult.Text = "Please select an LLM tab to submit your question.";
            return;
        }
        
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
            
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    llm = llmConfig?.ApiKey ?? llm.ToLower(),
                    prompt = prompt,
                    model = model
                };
                
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync("http://localhost:8000/askllm", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return responseText;
                }
                else
                {
                    return $"Error: HTTP {response.StatusCode} - {response.ReasonPhrase}";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error connecting to server: {ex.Message}";
        }
    }
    
    private void RefreshLLMListFromConfig()
    {
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
    }
}