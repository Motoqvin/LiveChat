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
    
    public async Task<List<MessageDto>> GetMessagesAsync(string room)
    {
        string key = $"room:{room}:messages";
        var messages = await _db.ListRangeAsync(key);

        return messages
            .Select(m => JsonSerializer.Deserialize<MessageDto>(m!)!)
            .ToList();
    }

    public async Task SaveMessageAsync(MessageDto message)
    {
        string key = $"room:{message.Room}:messages";
        string json = JsonSerializer.Serialize(message);

        await _db.ListRightPushAsync(key, json);

        await _db.ListTrimAsync(key, -MaxMessages, -1);
    }
}