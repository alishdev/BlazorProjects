namespace ChatAPI.Models;

using System;

// Represents a single message in the conversation history
public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Represents the incoming request from the WordPress plugin
public class ChatRequest
{
    public List<ChatMessage>? ChatHistory { get; set; }
    public string? UserIpAddress { get; set; }
}

// Represents the response we send back
public class ChatResponse
{
    public string? Response { get; set; }
}