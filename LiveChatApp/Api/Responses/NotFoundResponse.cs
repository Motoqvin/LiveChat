namespace LiveChatApp.Responses;
public class NotFoundResponse
{
    public string? Parameter { get; set; }
    public string Message { get; set; }

    public NotFoundResponse(string parameter, string message)
    {
        this.Message = message;
        this.Parameter = parameter;
    }

    public NotFoundResponse(string message)
    {
        this.Message = message;
    }
}