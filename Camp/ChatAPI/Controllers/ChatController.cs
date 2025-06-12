using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ChatAPI.Models;
using ChatAPI.Services;
using System.Linq;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // This will make the endpoint /api/chat
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IChatService _chatService;
        private readonly string? _apiKey;

        public ChatController(IConfiguration configuration, IChatService chatService)
        {
            _configuration = configuration;
            _chatService = chatService;
            // Retrieve the API key from configuration on startup
            _apiKey = _configuration.GetValue<string>("ChatSettings:ApiKey");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
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

            try
            {
                // --- 3. Get Response from Chat Service ---
                string responseMessage = await _chatService.GetResponseAsync(request.ChatHistory);

                // --- 4. Prepare and Send Response ---
                var response = new ChatResponse
                {
                    Response = responseMessage
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the error in production
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        [HttpGet("TestAPI")]
        public async Task<IActionResult> TestAPI()
        {
            try
            {
                // Create a sample chat history
                var chatHistory = new List<ChatMessage>
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = "Hi, I'd like to schedule a call to discuss your services."
                    }
                };

                // Create the request object
                var request = new ChatRequest
                {
                    ChatHistory = chatHistory,
                    UserIpAddress = "127.0.0.1" // Local testing IP
                };

                // Add the API key to headers
                Request.Headers.Add("Authorization", $"Bearer {_apiKey}");

                // Call the Post endpoint
                var result = await Post(request);

                // Return the result
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while testing the API.", error = ex.Message });
            }
        }
    }
}