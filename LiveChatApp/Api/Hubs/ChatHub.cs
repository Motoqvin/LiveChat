using LiveChatApp.Dtos;
using LiveChatApp.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LiveChatApp.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _service;
    public ChatHub(IChatService service)
    {
        _service = service;
    }

    public async Task JoinRoom(string room)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);

        var history = await _service.GetMessagesAsync(room);
        await Clients.Caller.SendAsync("LoadHistory", history);
    }

    public async Task SendMessage(MessageDto message)
    {
        await _service.SaveMessageAsync(message);

        await Clients.Group(message.Room)
            .SendAsync("ReceiveMessage", message);
        
        await Clients.AllExcept(Context.ConnectionId).SendAsync("RoomActivity", message.Room);
    }

    public async Task LeaveRoom(string room)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);

        var userName = Context.User?.Identity?.Name ?? "Unknown";
        await Clients.Group(room).SendAsync("SystemMessage", $"{userName} left the room.");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}