using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CVMatcherApp.Api.Extensions;
using LiveChatApp.Dtos;
using LiveChatApp.Hubs;
using LiveChatApp.Models;
using LiveChatApp.Options;
using LiveChatApp.Services;
using LiveChatApp.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false"));

builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<JwtOptions>>().Value);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR().AddStackExchangeRedis(options =>
    {
        var redisConnectionString = "localhost:6379,abortConnect=false";
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }
        options.Configuration = ConfigurationOptions.Parse(redisConnectionString);
    });

builder.Services.AddOptions<JwtOptions>().Configure(options =>
{
    options.SignatureKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "";
    options.Audience = "MyAudience";
    options.LifetimeInMinutes = 60;
    options.Issuer = "MyIssuer";
});

builder.Services.InitAspnetIdentity(builder.Configuration);
builder.Services.InitAuth();
builder.Services.InitSwagger();

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IChatService, ChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.MapPost("/register", async (
    [FromBody] RegisterDto dto,
    UserManager<User> userManager) =>
{
    var user = new User { UserName = dto.Username, Email = dto.Email };
    var result = await userManager.CreateAsync(user, dto.Password);

    if (!result.Succeeded)
        return Results.BadRequest(result.Errors);

    return Results.Ok("User registered.");
});

app.MapPost("/login", async ([FromBody] LoginDto dto,
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    JwtOptions jwtOptions) =>
{
    var foundUser = await userManager.FindByEmailAsync(dto.Login);

    if (foundUser == null)
    {
        return Results.BadRequest("Incorrect Login or Password");
    }

    var signInResult = await signInManager.PasswordSignInAsync(foundUser, dto.Password, true, true);

    if (signInResult.IsLockedOut)
    {
        return Results.BadRequest("User locked");
    }

    if (signInResult.Succeeded == false)
    {
        return Results.BadRequest("Incorrect Login or Password");
    }

    var roles = await userManager.GetRolesAsync(foundUser);


    var keyStr = jwtOptions.SignatureKey;
    var keyBytes = Encoding.ASCII.GetBytes(keyStr);

    var signingKey = new SymmetricSecurityKey(keyBytes);
    var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    var claims = roles.Select(role => new Claim(ClaimTypes.Role, role))
        .Append(new Claim(ClaimTypes.NameIdentifier, foundUser.Id))
        .Append(new Claim(ClaimTypes.Name, dto.Login))
        .Append(new Claim(ClaimTypes.Email, dto.Login));

    var token = new JwtSecurityToken(
        issuer: jwtOptions.Issuer,
        audience: jwtOptions.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(jwtOptions.LifetimeInMinutes),
        signingCredentials: signingCredentials
    );

    var handler = new JwtSecurityTokenHandler();
    var tokenStr = handler.WriteToken(token);

    return Results.Ok(new { token = tokenStr, Username = foundUser.UserName, foundUser.Email });
});

app.MapPost("/api/messages", [Authorize] async (
    MessageDto dto,
    IChatService chatService,
    IHubContext<ChatHub> hubContext) =>
{
    await chatService.SaveMessageAsync(dto);
    await hubContext.Clients.Group(dto.Room).SendAsync("ReceiveMessage", dto);
    return Results.Ok();
});

app.MapGet("/api/messages/{room}", async (
    string room,
    IChatService chatService) =>
{
    var messages = await chatService.GetMessagesAsync(room);
    return Results.Ok(messages);
});

app.Run();