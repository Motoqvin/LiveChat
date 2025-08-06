namespace LiveChatApp.Dtos;

public class MessageDto
{
    public required string User { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
}