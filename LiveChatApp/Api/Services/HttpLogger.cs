using LiveChatApp.Data;
using LiveChatApp.Models;
using LiveChatApp.Services.Base;

namespace LiveChatApp.Services;

public class HttpLogger : IHttpLogger
{
    private readonly ChatDbContext dbContext;
    public HttpLogger(ChatDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task LogAsync(HttpLog log)
    {
        if (log == null)
        {
            throw new ArgumentNullException(nameof(log), "Log cannot be null");
        }

        dbContext.Logs.Add(log);
        dbContext.SaveChanges();
        return Task.CompletedTask;
    }
}