using Microsoft.SemanticKernel;

namespace SemanticKernelPlayground;

public class PromptTest
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
        
        string language = "Russian";
        string history = @"I'm traveling with my kids and one of them 
    has a peanut allergy.";

        string prompt = @$"Consider the traveler's background:
    ${history}
    Create a list of helpful phrases and words in 
    ${language} a traveler would find useful.
    Group phrases by category. Include common direction 
    words. Display the phrases in the following format: 
    Hello - Ciao [chow]";

        var result = await kernel.InvokePromptAsync(prompt);
        Console.WriteLine(result);
    }
}