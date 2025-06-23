namespace TestLLM;

public class LLM
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;
    
    // Parameterless constructor for JSON deserialization
    public LLM()
    {
    }
    
    public LLM(string name)
    {
        Name = name;
    }
    
    public LLM(string name, string description, bool enabled, string apiKey, string defaultModel)
    {
        Name = name;
        Description = description;
        Enabled = enabled;
        ApiKey = apiKey;
        DefaultModel = defaultModel;
    }
} 