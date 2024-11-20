using Microsoft.SemanticKernel;

namespace LinkableAssets;

public class FindGoodDomain
{
    public async Task<List<string>> FindDomains()
    {
        // List of domains to check
        string openApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        // Create kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            apiKey: openApiKey,
            modelId: "gpt-4o-mini" 
        );
        
        var kernel = builder.Build();
        string prompt = "Give me a comma separated list of artifacts from Tolkien's Middle Earth.";
        var result = await kernel.InvokePromptAsync(prompt);
        string[] artifacts = result.ToString().Split(',');
        
        // check each domain
        List<string> domains = new List<string>();
        using (HttpClient client = new HttpClient())
        {
            foreach (string art in artifacts)
            {
                Console.WriteLine($"Checking {art}.com");
                try
                {
                    string domain = $"{art}.com";
                    var response = await client.GetAsync(domain);
                    if (response.IsSuccessStatusCode)
                    {
                        domains.Add(domain);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        return domains;
    }
}