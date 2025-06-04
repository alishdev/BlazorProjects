namespace LibrarianAPI.Models;

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
} 