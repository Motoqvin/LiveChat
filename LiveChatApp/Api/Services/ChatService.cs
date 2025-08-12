using System.Text.Json;
using LiveChatApp.Dtos;
using LiveChatApp.Services.Base;
using StackExchange.Redis;

namespace LiveChatApp.Services;

public class ChatService : IChatService
{
    private readonly IDatabase _db;
    private const int MaxMessages = 50;

    public ChatService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }
    
    public async Task SaveMessageAsync(MessageDto message)
    {
        var json = JsonSerializer.Serialize(message);

        await _db.ListRightPushAsync($"chat:{message.Room}", json);
    }

    public async Task<List<MessageDto>> GetMessagesAsync(string room)
    {
        var messages = await _db.ListRangeAsync($"chat:{room}", -MaxMessages, -1);

        return messages
            .Select(x => JsonSerializer.Deserialize<MessageDto>(x!)!)
            .ToList();
    }
}