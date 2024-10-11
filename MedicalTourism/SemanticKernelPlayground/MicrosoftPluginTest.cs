using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;

namespace SemanticKernelPlayground;

#pragma warning disable SKEXP0050, CS8604
public class MicrosoftPluginTest
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
        builder.Plugins.AddFromType<TimePlugin>();
        var kernel = builder.Build();
        var currentDay = await kernel.InvokeAsync("TimePlugin", "DayOfWeek");
        Console.WriteLine(currentDay);
    }
}