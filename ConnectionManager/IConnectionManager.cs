//using Fleck;
//using System.Collections.Generic;

//public interface IConnectionManager
//{
//    void AddSocket(IWebSocketConnection socket);
//    void RemoveSocket(IWebSocketConnection socket);
//    string? GetUserId(IWebSocketConnection socket);
//    IWebSocketConnection? GetSocket(string userId);
//    void RegisterUser(string userId, IWebSocketConnection socket);
//    void JoinGroup(string groupId, IWebSocketConnection socket);
//    void LeaveGroup(string groupId, IWebSocketConnection socket);
//    IEnumerable<IWebSocketConnection> GetSocketsInGroup(string groupId);
//    IEnumerable<string> GetOnlineUserIds();
//    List<IWebSocketConnection> GetAllSockets();
//}