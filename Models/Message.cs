namespace LiveChatApp.Models;

public class Message
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}