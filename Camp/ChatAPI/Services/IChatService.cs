using System.Threading.Tasks;
using System.Collections.Generic;
using ChatAPI.Models;

namespace ChatAPI.Services
{
    public interface IChatService
    {
        Task<string> GetResponseAsync(List<ChatMessage> chatHistory);
        Task<bool> ValidateApiKeyAsync(string apiKey);
    }
} 