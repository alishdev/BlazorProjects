using Microsoft.SemanticKernel;

namespace SemanticKernelPlayground;
#pragma warning disable SKEXP0050, CS8604
public class BasicTest
{
    public async Task Test()
    {
        var openApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        // Create kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            apiKey: openApiKey,
            modelId: "gpt-3.5-turbo" // models are here: https://platform.openai.com/docs/models/gpt-4o-mini
        );
        var kernel = builder.Build();
        
        var result = await kernel.InvokePromptAsync(
            "Give me a list of breakfast foods with eggs and cheese");
        Console.WriteLine(result);
    }
}