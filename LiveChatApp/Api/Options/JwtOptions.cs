namespace LiveChatApp.Options;
public class JwtOptions
{
    public required string Issuer { get; set; } = string.Empty;
    public required string Audience { get; set; } = string.Empty;
    public required long LifetimeInMinutes { get; set; } = 60;
    public required string SignatureKey { get; set; } = string.Empty;
}