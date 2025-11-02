// Services/IWebSocketStateService.cs
using Fleck;

// Defines the contract for our state management
public interface IWebSocketStateService
{
    void AddSocket(IWebSocketConnection socket);
    void RemoveSocket(IWebSocketConnection socket);
    string? GetUserId(IWebSocketConnection socket);
    bool RegisterUser(string userId, IWebSocketConnection socket);
    List<string> GetOnlineUserIds();
    IWebSocketConnection? GetSocketByUserId(string userId);
    void JoinGroup(string groupId, IWebSocketConnection socket);
    void CreateGroup(string groupId, IEnumerable<string> memberUserIds);
    void LeaveGroup(string groupId, IWebSocketConnection socket);
    List<IWebSocketConnection> GetSocketsInGroup(string groupId);
    List<IWebSocketConnection> GetAllSockets();
}