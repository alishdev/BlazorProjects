using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace PinterestImageBuilder;

public class ImagePlugin
{
    [KernelFunction ("CreateImage")]
    [Description("Create an image from a prompt.")]
    public async Task CreateImage(Kernel kernel, [Description("Prompt to create an image from.")] string prompt)
    {
        // create image from prompt
        Task task = Task.CompletedTask;
    }
}