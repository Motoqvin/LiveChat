namespace LiveChatApp.Models;
public class HttpLog
{
    public required Guid Id { get; set; }
    public required string Url { get; set; }
    public string? RequestBody { get; set; }
    public string? RequestHeaders { get; set; }
    public int MethodType { get; set; }
    public string? ResponseBody { get; set; }
    public string? ResponseHeaders { get; set; }
    public int StatusCode { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public required string ClientIp { get; set; }
}