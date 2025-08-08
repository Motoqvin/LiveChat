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

        var userName = Context.User?.Identity?.Name ?? "Unknown";

        await Clients.Group(room).SendAsync("SystemMessage", $"{userName} joined the room.");
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);

        var mes = new MessageDto
        {
            User = user,
            Message = message,
            Room = Context.GetHttpContext()?.Request.Query["room"].ToString() ?? string.Empty
        };

        await _service.SaveMessageAsync(mes);
        
        await Clients.Group(mes.Room).SendAsync("ReceiveMessage", mes);
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