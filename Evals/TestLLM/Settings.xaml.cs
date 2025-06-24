using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using System.Text;

namespace TestLLM;

public partial class Settings : ContentView
{
    private static readonly ILogger _logger = LoggingService.GetLogger<Settings>();
    private ObservableCollection<LLM> _llmList;
    private Dictionary<string, CheckBox> _checkBoxes;
    private MainPage? _mainPage; // Reference to MainPage to access responses

    public ObservableCollection<string> MaxTokensOptions { get; set; } = new() { "1000", "10000", "28000" };
    public string SelectedMaxTokens { get; set; } = "1000";
    
    // Event to notify parent when checkboxes change
    public event EventHandler<CheckBoxChangedEventArgs>? CheckBoxChanged;

    public Settings()
    {
        _logger.LogInformation("Initializing Settings control");
        InitializeComponent();
        
        // Initialize collections
        _logger.LogDebug("Loading LLMs from config");
        _llmList = new ObservableCollection<LLM>(LLMConfigService.LoadLLMsFromConfig());
        _logger.LogInformation("Loaded {Count} LLMs from config", _llmList.Count);
        
        _checkBoxes = new Dictionary<string, CheckBox>();
        
        // Set binding context for DropDown
        BindingContext = this;
        MaxTokensDropDown.ItemsSource = MaxTokensOptions;
        MaxTokensDropDown.SelectedItem = SelectedMaxTokens;
        MaxTokensDropDown.SelectionChanged += (s, val) => {
            SelectedMaxTokens = val;
        };
        
        // Initialize UI
        InitializeSettingsCheckboxes();
        
        _logger.LogInformation("Settings control initialization completed");
    }

    private void InitializeSettingsCheckboxes()
    {
        _logger.LogDebug("Initializing settings checkboxes for {Count} LLMs", _llmList.Count);
        
        foreach (var llm in _llmList)
        {
            _logger.LogDebug("Creating checkbox for LLM: {Name} (NameAndModel: {NameAndModel})", llm.Name, llm.NameAndModel);
            
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
                Text = $"Model: {llm.DefaultModel}",
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
            
            // Add debugging for checkbox event
            checkBox.CheckedChanged += (sender, e) => 
            {
                _logger.LogInformation("Checkbox CheckedChanged event fired for {LLMName}: {IsChecked}", llm.NameAndModel, e.Value);
                OnCheckBoxChanged(llm.NameAndModel, e.Value);
            };
            
            // Add a tap gesture to the container that toggles the checkbox
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (sender, e) => 
            {
                _logger.LogInformation("Container tapped for {LLMName}, toggling checkbox", llm.NameAndModel);
                checkBox.IsChecked = !checkBox.IsChecked;
            };
            container.GestureRecognizers.Add(tapGesture);
            
            // Add a subtle shadow to make it look more interactive
            container.Shadow = new Shadow
            {
                Brush = Color.FromArgb("#00000010"),
                Offset = new Point(0, 2),
                Radius = 4
            };
            
            _checkBoxes[llm.Name] = checkBox;
            SettingsCheckboxes.Children.Add(container);
            _logger.LogDebug("Created checkbox for LLM: {Name}", llm.Name);
        }
        
        _logger.LogInformation("Initialized {Count} checkboxes", _checkBoxes.Count);
    }

    private void OnCheckBoxChanged(string llmName, bool isChecked)
    {
        _logger.LogInformation("OnCheckBoxChanged called for {LLMName}: {IsChecked}", llmName, isChecked);
        
        // Raise the event to notify the parent
        CheckBoxChanged?.Invoke(this, new CheckBoxChangedEventArgs(llmName, isChecked));
    }

    // Method to get the list of LLMs for tab creation
    public ObservableCollection<LLM> GetLLMList()
    {
        return _llmList;
    }

    // Method to get enabled LLMs (those with checked checkboxes)
    public List<LLM> GetEnabledLLMs()
    {
        return _llmList.Where(llm => 
            _checkBoxes.ContainsKey(llm.Name) && 
            _checkBoxes[llm.Name].IsChecked).ToList();
    }

    // Method to get the selected Max Tokens value
    public int GetMaxTokens()
    {
        if (int.TryParse(SelectedMaxTokens, out int maxTokens))
        {
            return maxTokens;
        }
        
        // Return default value if parsing fails
        return 1000;
    }

    // Method to set the Max Tokens value
    public void SetMaxTokens(int maxTokens)
    {
        var tokensString = maxTokens.ToString();
        if (MaxTokensOptions.Contains(tokensString))
        {
            SelectedMaxTokens = tokensString;
            MaxTokensDropDown.SelectedItem = tokensString;
        }
    }

    // Event handler for Merge Responses button
    private void OnMergeResponsesClicked(object sender, EventArgs e)
    {
        _logger.LogInformation("Merge Responses button clicked");
        
        try
        {
            // Get all enabled LLMs
            var enabledLLMs = GetEnabledLLMs();
            _logger.LogInformation("Found {Count} enabled LLMs for merging responses", enabledLLMs.Count);
            
            // 1. Collecting responses from all enabled LLM tabs
            var responses = new Dictionary<string, string>();
            
            if (_mainPage != null)
            {
                var currentResponses = _mainPage.GetCurrentResponses();
                _logger.LogInformation("Retrieved {Count} current responses from MainPage", currentResponses.Count);
                
                // Filter responses to only include enabled LLMs
                foreach (var llm in enabledLLMs)
                {
                    if (currentResponses.TryGetValue(llm.NameAndModel, out var response))
                    {
                        responses[llm.NameAndModel] = response;
                        _logger.LogInformation("Found response for enabled LLM: {Name}", llm.NameAndModel);
                    }
                    else
                    {
                        _logger.LogWarning("No response found for enabled LLM: {Name}", llm.NameAndModel);
                    }
                }
            }
            else
            {
                _logger.LogWarning("MainPage reference is null, cannot access responses");
            }
            
            if (responses.Count == 0)
            {
                _logger.LogWarning("No responses found for any enabled LLMs");
                return;
            }
            
            // 2. Merging the responses in some meaningful way
            var mergedResponse = MergeResponses(responses);
            _logger.LogInformation("Successfully merged {Count} responses", responses.Count);
            
            // 3. Displaying or storing the merged result
            DisplayMergedResponse(mergedResponse, responses);
            
            _logger.LogInformation("Merge Responses functionality completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while merging responses");
        }
    }

    // Method to merge responses from multiple LLMs
    private string MergeResponses(Dictionary<string, string> responses)
    {
        _logger.LogInformation("Starting to merge {Count} responses", responses.Count);
        
        var mergedContent = new StringBuilder();
        mergedContent.AppendLine("# 🔄 Merged LLM Responses");
        mergedContent.AppendLine();
        mergedContent.AppendLine($"**Generated on:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        mergedContent.AppendLine($"**Total LLMs:** {responses.Count}");
        mergedContent.AppendLine();
        
        // Add individual responses
        foreach (var kvp in responses.OrderBy(x => x.Key))
        {
            mergedContent.AppendLine($"## 🤖 {kvp.Key}");
            mergedContent.AppendLine();
            mergedContent.AppendLine(kvp.Value);
            mergedContent.AppendLine();
            mergedContent.AppendLine("---");
            mergedContent.AppendLine();
        }
        
        // Add summary section
        mergedContent.AppendLine("## 📊 Summary");
        mergedContent.AppendLine();
        mergedContent.AppendLine($"- **Total Responses:** {responses.Count}");
        mergedContent.AppendLine($"- **Average Response Length:** {responses.Values.Average(r => r.Length):F0} characters");
        mergedContent.AppendLine($"- **Longest Response:** {responses.Values.Max(r => r.Length)} characters");
        mergedContent.AppendLine($"- **Shortest Response:** {responses.Values.Min(r => r.Length)} characters");
        
        var mergedText = mergedContent.ToString();
        _logger.LogInformation("Merged response created with {Length} characters", mergedText.Length);
        
        return mergedText;
    }

    // Method to display the merged response
    private void DisplayMergedResponse(string mergedResponse, Dictionary<string, string> originalResponses)
    {
        _logger.LogInformation("Displaying merged response");
        
        // Log the merged response to debug window
        _logger.LogInformation("=== MERGED RESPONSE START ===");
        _logger.LogInformation(mergedResponse);
        _logger.LogInformation("=== MERGED RESPONSE END ===");
        
        // Also log individual response summaries for debugging
        foreach (var kvp in originalResponses)
        {
            var shortResponse = kvp.Value.Length > 100 ? kvp.Value.Substring(0, 100) + "..." : kvp.Value;
            _logger.LogInformation("Response from {LLM}: {ShortResponse}", kvp.Key, shortResponse);
        }
        
        // TODO: In a future enhancement, you could:
        // 1. Create a new tab to display the merged response
        // 2. Save the merged response to a file
        // 3. Show the merged response in a popup dialog
        // 4. Add it to the MainPage's response display
    }

    // Method to set the MainPage reference
    public void SetMainPage(MainPage mainPage)
    {
        _mainPage = mainPage;
        _logger.LogDebug("MainPage reference set in Settings control");
    }
}

// Event args for checkbox changes
public class CheckBoxChangedEventArgs : EventArgs
{
    public string LLMName { get; }
    public bool IsChecked { get; }

    public CheckBoxChangedEventArgs(string llmName, bool isChecked)
    {
        LLMName = llmName;
        IsChecked = isChecked;
    }
} 