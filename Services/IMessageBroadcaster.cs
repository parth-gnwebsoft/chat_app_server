// Services/IMessageBroadcaster.cs
using Fleck;

public interface IMessageBroadcaster
{
    void Send(IWebSocketConnection socket, string type, object payload);
    void SendToUser(string userId, string type, object payload);
    void Broadcast(string type, object payload);
    void BroadcastToGroup(string groupId, string type, object payload, IWebSocketConnection? excludeSocket = null);
}