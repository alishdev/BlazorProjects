using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ChatAPI.Models;
using System.Linq;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // This will make the endpoint /api/chat
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string? _apiKey;

        public ChatController(IConfiguration configuration)
        {
            _configuration = configuration;
            // Retrieve the API key from configuration on startup
            _apiKey = _configuration.GetValue<string>("ChatSettings:ApiKey");
        }

        [HttpPost]
        public IActionResult Post([FromBody] ChatRequest request)
        {
            // --- 1. Authentication ---
            // Get the Authorization header from the incoming request
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return Unauthorized(new { message = "Authorization header is missing." });
            }

            var headerValue = authHeader.FirstOrDefault();
            if (string.IsNullOrEmpty(headerValue) || !headerValue.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Malformed Authorization header." });
            }

            // Extract the key sent by the WordPress plugin
            var providedApiKey = headerValue.Substring("Bearer ".Length).Trim();

            // Compare it with the key stored securely in our configuration
            if (string.IsNullOrEmpty(providedApiKey) || providedApiKey != _apiKey)
            {
                return Unauthorized(new { message = "Invalid API Key." });
            }

            // --- 2. Input Validation ---
            if (request?.ChatHistory == null || !request.ChatHistory.Any())
            {
                return BadRequest(new { message = "Chat history is empty or invalid." });
            }

            // --- 3. Core Logic (The "AI") ---
            // Get the latest message from the user to process
            var lastUserMessage = request.ChatHistory.LastOrDefault(m => m.Role == "user");
            if (lastUserMessage == null)
            {
                return BadRequest(new { message = "No user message found in history." });
            }
            
            // Generate a response based on the user's message
            string responseMessage = GenerateSimpleResponse(lastUserMessage.Content);

            // --- 4. Prepare and Send Response ---
            var response = new ChatResponse
            {
                Response = responseMessage
            };

            return Ok(response);
        }

        /// <summary>
        /// This is a placeholder for your actual AI/bot logic.
        /// It can be replaced with calls to OpenAI, a database, or other services.
        /// </summary>
        private string GenerateSimpleResponse(string? userMessage)
        {
            if (userMessage != null)
            {
                string lowerUserMessage = userMessage.ToLower();

                if (lowerUserMessage.Contains("hello") || lowerUserMessage.Contains("hi"))
                {
                    return "Hello! How can I help you today?";
                }
                if (lowerUserMessage.Contains("price") || lowerUserMessage.Contains("cost"))
                {
                    return "Our pricing details can be found on the pricing page of our website.";
                }
                if (lowerUserMessage.Contains("support") || lowerUserMessage.Contains("help"))
                {
                    return "You can contact our support team at support@example.com.";
                }
                if (lowerUserMessage.Contains("bye"))
                {
                    return "Goodbye! Have a great day.";
                }
            }

            return "I'm sorry, I don't understand that question. Please try rephrasing or ask about pricing or support.";
        }
    }
}