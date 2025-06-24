using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace TestLLM;

public partial class Settings : ContentView
{
    private static readonly ILogger _logger = LoggingService.GetLogger<Settings>();
    private ObservableCollection<LLM> _llmList;
    private Dictionary<string, CheckBox> _checkBoxes;
    
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