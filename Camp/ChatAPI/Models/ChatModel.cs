namespace ChatAPI.Models;

// Represents a single message in the conversation history
public class ChatMessage
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

// Represents the incoming request from the WordPress plugin
public class ChatRequest
{
    public List<ChatMessage>? ChatHistory { get; set; }
}

// Represents the response we send back
public class ChatResponse
{
    public string? Response { get; set; }
}