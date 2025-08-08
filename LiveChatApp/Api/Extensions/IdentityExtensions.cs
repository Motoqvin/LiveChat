using LiveChatApp.Data;
using LiveChatApp.Models;
using LiveChatApp.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CVMatcherApp.Api.Extensions;

public static class IdentityExtensions
{
    public static void InitAspnetIdentity(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddDbContext<ChatDbContext>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("SQLDB");
            options.UseNpgsql(connectionString);
        });

        serviceCollection.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ChatDbContext>();
    }

    public async static Task SeedRolesAsync(this WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await roleManager.CreateAsync(new IdentityRole("User"));
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    public static void InitAuth(this IServiceCollection services)
    {
        var jwtOptions = services.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>().Value;
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;


        })
            .AddJwtBearer(options =>
            {
                var keyStr = jwtOptions.SignatureKey;
                System.Console.WriteLine(keyStr);
                var keyBytes = System.Text.Encoding.ASCII.GetBytes(keyStr);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });


    }
}