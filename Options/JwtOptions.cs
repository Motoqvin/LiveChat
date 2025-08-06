namespace LiveChatApp.Options;
public class JwtOptions
{
    public required string Issuer { get; set; } = "MyIssuer";
    public required string Audience { get; set; } = "MyAudience";
    public required long LifetimeInMinutes { get; set; } = 60;
    public required string SignatureKey { get; set; } = Environment.GetEnvironmentVariable("JWT_KEY")!;
}