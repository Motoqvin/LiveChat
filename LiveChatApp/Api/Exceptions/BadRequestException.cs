namespace LiveChatApp.Exceptions;
public class BadRequestException : Exception
{
    public string Param { get; }
    public BadRequestException(string message, string param) : base(message)
    {
        Param = param;
    }

}