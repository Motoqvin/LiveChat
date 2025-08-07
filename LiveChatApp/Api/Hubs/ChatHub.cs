using Microsoft.AspNetCore.SignalR;

namespace LiveChatApp.Hubs;
public class ChatHub : Hub
{
    public async Task JoinRoom(string room)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);

        var userName = Context.User?.Identity?.Name ?? "Unknown";

        await Clients.Group(room).SendAsync("SystemMessage", $"{userName} joined the room.");
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
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