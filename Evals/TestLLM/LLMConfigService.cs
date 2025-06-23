using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace TestLLM;

public class LLMConfig
{
    [JsonPropertyName("llms")]
    public List<LLM> LLMs { get; set; } = new List<LLM>();
}

public class LLMConfigService
{
    private static readonly ILogger _logger = LoggingService.GetLogger<LLMConfigService>();
    private const string ConfigFileName = "llm_config.json";
    
    public static List<LLM> LoadLLMsFromConfig()
    {
        try
        {
            var configPath = Path.Combine(FileSystem.AppDataDirectory, ConfigFileName);
            _logger.LogDebug("Looking for config file at: {ConfigPath}", configPath);
            
            // If config file doesn't exist in app data, copy from app bundle
            if (!File.Exists(configPath))
            {
                _logger.LogInformation("Config file not found in app data, copying from bundle...");
                CopyConfigFromBundle(configPath);
            }
            
            if (File.Exists(configPath))
            {
                var jsonContent = File.ReadAllText(configPath);
                _logger.LogDebug("Config file content length: {Length}", jsonContent.Length);
                _logger.LogDebug("Config file content: {Content}", jsonContent);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                try
                {
                    var config = JsonSerializer.Deserialize<LLMConfig>(jsonContent, options);
                    
                    _logger.LogInformation("Deserialized config: {Count} LLMs found", config?.LLMs?.Count ?? 0);
                    
                    if (config?.LLMs != null)
                    {
                        var enabledLLMs = config.LLMs.Where(llm => llm.Enabled).ToList();
                        _logger.LogInformation("Enabled LLMs: {Count}", enabledLLMs.Count);
                        return enabledLLMs;
                    }
                    else
                    {
                        _logger.LogWarning("Config.LLMs is null");
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "JSON deserialization error: {Message}", jsonEx.Message);
                    _logger.LogError("JSON error path: {Path}, Line: {Line}", jsonEx.Path, jsonEx.LineNumber);
                }
            }
            else
            {
                _logger.LogWarning("Config file not found after copy attempt");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading LLM config: {Message}", ex.Message);
        }
        
        // Fallback to default LLMs if config loading fails
        _logger.LogWarning("Using fallback default LLMs");
        return GetDefaultLLMs();
    }
    
    private static void CopyConfigFromBundle(string targetPath)
    {
        try
        {
            _logger.LogDebug("Attempting to copy config from bundle to: {TargetPath}", targetPath);
            
            // Try to get the file from the bundle
            var stream = FileSystem.OpenAppPackageFileAsync(ConfigFileName).Result;
            if (stream == null)
            {
                _logger.LogError("Stream is null - file not found in bundle");
                return;
            }
            
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            _logger.LogDebug("Read content from bundle: {Length} characters", content.Length);
            
            if (string.IsNullOrEmpty(content))
            {
                _logger.LogError("Content is empty from bundle");
                return;
            }
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created directory: {Directory}", directory);
            }
            
            File.WriteAllText(targetPath, content);
            _logger.LogInformation("Successfully copied config file from bundle");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying config from bundle: {Message}", ex.Message);
        }
    }
    
    private static List<LLM> GetDefaultLLMs()
    {
        _logger.LogInformation("Creating default LLM list");
        return new List<LLM>
        {
            new LLM("GPT-4", "OpenAI's GPT-4 model", true, "openai", "gpt-4"),
            new LLM("Claude", "Anthropic's Claude model", true, "anthropic", "claude-3-sonnet-20240229"),
            new LLM("Gemini", "Google's Gemini model", true, "gemini", "gemini-pro"),
            new LLM("Llama", "Meta's Llama model", true, "llama", "llama-2-70b")
        };
    }
    
    public static void SaveLLMsToConfig(List<LLM> llms)
    {
        try
        {
            _logger.LogInformation("Saving {Count} LLMs to config", llms.Count);
            var config = new LLMConfig { LLMs = llms };
            var jsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            var configPath = Path.Combine(FileSystem.AppDataDirectory, ConfigFileName);
            File.WriteAllText(configPath, jsonContent);
            _logger.LogInformation("Successfully saved LLM config to {Path}", configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving LLM config: {Message}", ex.Message);
        }
    }
    
    // Test method to verify JSON deserialization
    public static void TestJsonDeserialization()
    {
        try
        {
            _logger.LogDebug("Testing JSON deserialization...");
            
            var testJson = @"{
  ""llms"": [
    {
      ""name"": ""GPT-4"",
      ""description"": ""OpenAI's GPT-4 model"",
      ""enabled"": true,
      ""apiKey"": ""openai"",
      ""defaultModel"": ""gpt-4""
    }
  ]
}";
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var config = JsonSerializer.Deserialize<LLMConfig>(testJson, options);
            _logger.LogInformation("Test deserialization: {Count} LLMs found", config?.LLMs?.Count ?? 0);
            
            if (config?.LLMs?.Count > 0)
            {
                var llm = config.LLMs[0];
                _logger.LogInformation("First LLM: {Name}, Enabled: {Enabled}", llm.Name, llm.Enabled);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test deserialization failed: {Message}", ex.Message);
        }
    }
    
    // Test method to check if bundled file exists
    public static void TestBundledFileAccess()
    {
        try
        {
            _logger.LogDebug("Testing bundled file access...");
            var stream = FileSystem.OpenAppPackageFileAsync(ConfigFileName).Result;
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            _logger.LogDebug("Bundled file content length: {Length}", content.Length);
            _logger.LogDebug("First 100 chars: {Content}", content.Substring(0, Math.Min(100, content.Length)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bundled file access failed: {Message}", ex.Message);
        }
    }
    
    // Test method to verify serialization/deserialization round trip
    public static void TestSerializationRoundTrip()
    {
        try
        {
            _logger.LogDebug("Testing serialization round trip...");
            
            // Create test data
            var testLLMs = new List<LLM>
            {
                new LLM("Test-LLM", "Test Description", true, "test-api", "test-model")
            };
            
            var testConfig = new LLMConfig { LLMs = testLLMs };
            
            // Serialize
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(testConfig, options);
            _logger.LogDebug("Serialized JSON: {Json}", json);
            
            // Deserialize
            var deserializedConfig = JsonSerializer.Deserialize<LLMConfig>(json, options);
            _logger.LogInformation("Deserialized config has {Count} LLMs", deserializedConfig?.LLMs?.Count ?? 0);
            
            if (deserializedConfig?.LLMs?.Count > 0)
            {
                var llm = deserializedConfig.LLMs[0];
                _logger.LogInformation("Deserialized LLM: {Name}, {Description}, Enabled: {Enabled}", 
                    llm.Name, llm.Description, llm.Enabled);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Serialization round trip test failed: {Message}", ex.Message);
        }
    }
} 