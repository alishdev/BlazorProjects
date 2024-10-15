using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToImage;
using OpenAI.Images;

namespace PinterestImageBuilder;

#pragma warning disable SKEXP0001, SKEXP0010
public class AIImageBuilder
{
    [KernelFunction ("CreateImage")]
    [Description("Create an image from a prompt.")]
    public async Task<IReadOnlyList<ImageContent>> CreateImage([Description("Prompt to create an image from.")] string prompt)
    {
        var openApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
        // Create kernel
        /*var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            apiKey: openApiKey,
            modelId: "dall-e-3" // 3.5-turbo does not allow calling plugins
        );
        var kernel = builder.Build();*/
        
        var textToImageService = new OpenAITextToImageService(openApiKey, null, "dall-e-3", null, null);
        //var textToImageService = new OpenAITextToImageService(openApiKey, 1024, 1024, kernel);
        var image = await textToImageService.GetImageContentsAsync(prompt, null, null);
        return image;

        /*var imageService = kernel.GetRequiredService<ITextToImageService>();
        var url= await imageService.GenerateImageAsync("a happy monkey sitting in a tree, in watercolor",1024,1024);
        return url;*/
    }
}