using LiveChatApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LiveChatApp.Data;

public class ChatDbContext : IdentityDbContext<User, IdentityRole, string>
{
    public DbSet<HttpLog> Logs { get; set; }
    public ChatDbContext(DbContextOptions options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql("Host=localhost;Port=5032;Database=postgres;Username=postgres;Password=mysecretpassword;");
    }
}