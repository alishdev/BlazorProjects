using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ChatAPI.Services
{
    public class SemanticKernelChatService : IChatService
    {
        private readonly IConfiguration _configuration;
        private readonly Kernel _kernel;
        private readonly string _apiKey;
        private readonly string _systemPrompt = @"
You are a helpful assistant whose main goal is to help users schedule a call with the website owner.
Your responses should be professional and focused on:
1. Understanding when the user wants to schedule a call
2. Collecting necessary information (preferred time, topic, contact details)
3. Explaining the next steps in the scheduling process
4. If the user's query is not about scheduling a call, politely guide them towards scheduling one

Always maintain a professional and helpful tone.";

        public SemanticKernelChatService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration.GetValue<string>("ChatSettings:OpenAIApiKey") ?? 
                throw new InvalidOperationException("OpenAI API key not found in configuration");

            // Create kernel builder and add OpenAI chat completion
            var kernelBuilder = Kernel.CreateBuilder();
            
            // Configure OpenAI
            kernelBuilder.Services.AddOpenAIChatCompletion("gpt-3.5-turbo", _apiKey);
            
            _kernel = kernelBuilder.Build();
        }

        public async Task<string> GetResponseAsync(List<ChatMessage> messages)
        {
            try
            {
                // Get chat completion service
                var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

                // Create chat history
                var chatHistory = new ChatHistory();
                
                // Add system message first
                chatHistory.AddMessage(AuthorRole.System, _systemPrompt);

                // Add chat history
                foreach (var message in messages)
                {
                    var role = message.Role.ToLower() switch
                    {
                        "user" => AuthorRole.User,
                        "assistant" => AuthorRole.Assistant,
                        "system" => AuthorRole.System,
                        _ => throw new ArgumentException($"Invalid role: {message.Role}")
                    };
                    
                    chatHistory.AddMessage(role, message.Content);
                }

                // Get response
                var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                
                return result.Content;
            }
            catch (Exception ex)
            {
                // Log the error in production
                Console.WriteLine($"Error generating response: {ex.Message}");
                return "I apologize, but I'm having trouble processing your request right now. Please try again later.";
            }
        }

        public Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            return Task.FromResult(apiKey == _apiKey);
        }
    }
} 