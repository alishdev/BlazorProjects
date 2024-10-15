using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace PinterestImageBuilder;

public class PluginsTest
{
    public async Task Test()
    {
        var openApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
        // Create kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            apiKey: openApiKey,
            modelId: "gpt-4-turbo" // 3.5-turbo does not allow calling plugins
        );
        
        // add plugins
        // builder.Plugins.AddFromType<NewsPlugin>();
        // builder.Plugins.AddFromType<TodayPlugin>();
        // builder.Plugins.AddFromType<ArchivePlugin>();
        
        var kernel = builder.Build();

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        ChatHistory chatHistory = new ChatHistory(
            // this is how you can set the persona
            "You are Donald Trump." +
            "If user asks for news ask which category they want." +
            "If user asks for the current date and time, provide it." +
            "Ask user if they want to archive data."
            );

        while (true)
        {
            Console.WriteLine("Prompt:");
            string? prompt = Console.ReadLine();
            if (string.IsNullOrEmpty(prompt))
                continue;
            if (prompt == "exit")
                break;
            chatHistory.AddUserMessage(prompt);
            
            var completion =  chatService.GetStreamingChatMessageContentsAsync(chatHistory,
                executionSettings: new OpenAIPromptExecutionSettings()
                {
                    //MaxTokens = 100,
                    //Temperature = 0.5,
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions  // allow plugin functions to be called automatically
                },
                kernel: kernel);

            string airesponse = "";
            await foreach (var content in completion)
            {
                Console.Write(content);
                airesponse += content;
            }
            chatHistory.AddAssistantMessage(airesponse);
            Console.WriteLine();
        }
    }
}