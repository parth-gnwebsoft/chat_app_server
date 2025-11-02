// Services/MessageBroadcaster.cs
using Fleck;
using System.Text.Json;

public class MessageBroadcaster : IMessageBroadcaster
{
    private readonly IWebSocketStateService _stateService;

    public MessageBroadcaster(IWebSocketStateService stateService)
    {
        _stateService = stateService;
    }

    public void Send(IWebSocketConnection socket, string type, object payload)
    {
        var messageObject = new { type, payload };
        var jsonMessage = JsonSerializer.Serialize(messageObject);
        socket.Send(jsonMessage);
    }

    public void SendToUser(string userId, string type, object payload)
    {
        if (_stateService.GetSocketByUserId(userId) is { } socket)
        {
            Send(socket, type, payload);
        }
    }

    public void Broadcast(string type, object payload)
    {
        var messageObject = new { type, payload };
        var jsonMessage = JsonSerializer.Serialize(messageObject);
        var socketsCopy = _stateService.GetAllSockets();

        foreach (var s in socketsCopy)
        {
            s.Send(jsonMessage);
        }
    }

    public void BroadcastToGroup(string groupId, string type, object payload, IWebSocketConnection? excludeSocket = null)
    {
        if (string.IsNullOrEmpty(groupId)) return;

        var messageObject = new { type, payload };
        var jsonMessage = JsonSerializer.Serialize(messageObject);
        var groupSocketsCopy = _stateService.GetSocketsInGroup(groupId);

        foreach (var s in groupSocketsCopy)
        {
            if (s != excludeSocket)
            {
                s.Send(jsonMessage);
            }
        }
    }
}