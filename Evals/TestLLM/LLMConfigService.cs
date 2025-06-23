using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestLLM;

public class LLMConfig
{
    [JsonPropertyName("llms")]
    public List<LLM> LLMs { get; set; } = new List<LLM>();
}

public class LLMConfigService
{
    private const string ConfigFileName = "llm_config.json";
    
    public static List<LLM> LoadLLMsFromConfig()
    {
        try
        {
            var configPath = Path.Combine(FileSystem.AppDataDirectory, ConfigFileName);
            System.Diagnostics.Debug.WriteLine($"Looking for config file at: {configPath}");
            
            // If config file doesn't exist in app data, copy from app bundle
            if (!File.Exists(configPath))
            {
                System.Diagnostics.Debug.WriteLine("Config file not found in app data, copying from bundle...");
                CopyConfigFromBundle(configPath);
            }
            
            if (File.Exists(configPath))
            {
                var jsonContent = File.ReadAllText(configPath);
                System.Diagnostics.Debug.WriteLine($"Config file content length: {jsonContent.Length}");
                System.Diagnostics.Debug.WriteLine($"Config file content: {jsonContent}");
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                try
                {
                    var config = JsonSerializer.Deserialize<LLMConfig>(jsonContent, options);
                    
                    System.Diagnostics.Debug.WriteLine($"Deserialized config: {config?.LLMs?.Count ?? 0} LLMs found");
                    
                    if (config?.LLMs != null)
                    {
                        var enabledLLMs = config.LLMs.Where(llm => llm.Enabled).ToList();
                        System.Diagnostics.Debug.WriteLine($"Enabled LLMs: {enabledLLMs.Count}");
                        return enabledLLMs;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Config.LLMs is null");
                    }
                }
                catch (JsonException jsonEx)
                {
                    System.Diagnostics.Debug.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"JSON error path: {jsonEx.Path}");
                    System.Diagnostics.Debug.WriteLine($"JSON error line number: {jsonEx.LineNumber}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Config file not found after copy attempt");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading LLM config: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        // Fallback to default LLMs if config loading fails
        System.Diagnostics.Debug.WriteLine("Using fallback default LLMs");
        return GetDefaultLLMs();
    }
    
    private static void CopyConfigFromBundle(string targetPath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Attempting to copy config from bundle to: {targetPath}");
            
            // Try to get the file from the bundle
            var stream = FileSystem.OpenAppPackageFileAsync(ConfigFileName).Result;
            if (stream == null)
            {
                System.Diagnostics.Debug.WriteLine("Stream is null - file not found in bundle");
                return;
            }
            
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            System.Diagnostics.Debug.WriteLine($"Read content from bundle: {content.Length} characters");
            
            if (string.IsNullOrEmpty(content))
            {
                System.Diagnostics.Debug.WriteLine("Content is empty from bundle");
                return;
            }
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(targetPath, content);
            System.Diagnostics.Debug.WriteLine("Successfully copied config file from bundle");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying config from bundle: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    private static List<LLM> GetDefaultLLMs()
    {
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
            var config = new LLMConfig { LLMs = llms };
            var jsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            var configPath = Path.Combine(FileSystem.AppDataDirectory, ConfigFileName);
            File.WriteAllText(configPath, jsonContent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving LLM config: {ex.Message}");
        }
    }
    
    // Test method to verify JSON deserialization
    public static void TestJsonDeserialization()
    {
        try
        {
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
            System.Diagnostics.Debug.WriteLine($"Test deserialization: {config?.LLMs?.Count ?? 0} LLMs found");
            
            if (config?.LLMs?.Count > 0)
            {
                var llm = config.LLMs[0];
                System.Diagnostics.Debug.WriteLine($"First LLM: {llm.Name}, Enabled: {llm.Enabled}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Test deserialization failed: {ex.Message}");
        }
    }
    
    // Test method to check if bundled file exists
    public static void TestBundledFileAccess()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Testing bundled file access...");
            var stream = FileSystem.OpenAppPackageFileAsync(ConfigFileName).Result;
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            System.Diagnostics.Debug.WriteLine($"Bundled file content length: {content.Length}");
            System.Diagnostics.Debug.WriteLine($"First 100 chars: {content.Substring(0, Math.Min(100, content.Length))}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Bundled file access failed: {ex.Message}");
        }
    }
    
    // Test method to verify serialization/deserialization round trip
    public static void TestSerializationRoundTrip()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Testing serialization round trip...");
            
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
            System.Diagnostics.Debug.WriteLine($"Serialized JSON: {json}");
            
            // Deserialize
            var deserializedConfig = JsonSerializer.Deserialize<LLMConfig>(json, options);
            System.Diagnostics.Debug.WriteLine($"Deserialized config has {deserializedConfig?.LLMs?.Count ?? 0} LLMs");
            
            if (deserializedConfig?.LLMs?.Count > 0)
            {
                var llm = deserializedConfig.LLMs[0];
                System.Diagnostics.Debug.WriteLine($"Deserialized LLM: {llm.Name}, {llm.Description}, Enabled: {llm.Enabled}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Serialization round trip test failed: {ex.Message}");
        }
    }
} 