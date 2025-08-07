using LiveChatApp.Models;

namespace LiveChatApp.Services.Base;
public interface IHttpLogger
{
    Task LogAsync(HttpLog log);
}