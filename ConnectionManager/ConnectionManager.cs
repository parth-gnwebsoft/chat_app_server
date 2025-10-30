//using Fleck;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;

//public class ConnectionManager : IConnectionManager
//{
//    private readonly List<IWebSocketConnection> _allSockets = new();
//    private readonly ConcurrentDictionary<string, IWebSocketConnection> _onlineUsers = new();
//    private readonly ConcurrentDictionary<string, List<IWebSocketConnection>> _groups = new();
//    private readonly object _socketLock = new();
//    private readonly object _groupLock = new();

//    public void AddSocket(IWebSocketConnection socket)
//    {
//        lock (_socketLock)
//        {
//            _allSockets.Add(socket);
//        }
//        Console.WriteLine($"Socket connected: {socket.ConnectionInfo.Id}");
//    }

//    public void RemoveSocket(IWebSocketConnection socket)
//    {
//        lock (_socketLock)
//        {
//            _allSockets.Remove(socket);
//        }

//        var userId = GetUserId(socket);
//        if (userId != null)
//        {
//            _onlineUsers.TryRemove(userId, out _);
//            Console.WriteLine($"User {userId} removed.");
//        }

//        lock (_groupLock)
//        {
//            foreach (var group in _groups.Values)
//            {
//                group.Remove(socket);
//            }
//        }
//        Console.WriteLine($"Socket disconnected: {socket.ConnectionInfo.Id}");
//    }

//    public string? GetUserId(IWebSocketConnection socket) =>
//        _onlineUsers.FirstOrDefault(x => x.Value == socket).Key;

//    public IWebSocketConnection? GetSocket(string userId) =>
//        _onlineUsers.TryGetValue(userId, out var socket) ? socket : null;

//    public IEnumerable<string> GetOnlineUserIds() => _onlineUsers.Keys.ToList();

//    public List<IWebSocketConnection> GetAllSockets()
//    {
//        lock (_socketLock)
//        {
//            return new List<IWebSocketConnection>(_allSockets);
//        }
//    }

//    public void RegisterUser(string userId, IWebSocketConnection socket)
//    {
//        _onlineUsers[userId] = socket;
//        Console.WriteLine($"User {userId} registered with socket {socket.ConnectionInfo.Id}");
//    }

//    public void JoinGroup(string groupId, IWebSocketConnection socket)
//    {
//        lock (_groupLock)
//        {
//            if (!_groups.ContainsKey(groupId))
//            {
//                _groups[groupId] = new List<IWebSocketConnection>();
//            }
//            if (!_groups[groupId].Contains(socket))
//            {
//                _groups[groupId].Add(socket);
//            }
//        }
//        Console.WriteLine($"Socket {socket.ConnectionInfo.Id} joined group {groupId}");
//    }

//    public void LeaveGroup(string groupId, IWebSocketConnection socket)
//    {
//        lock (_groupLock)
//        {
//            if (_groups.ContainsKey(groupId))
//            {
//                _groups[groupId].Remove(socket);
//            }
//        }
//        Console.WriteLine($"Socket {socket.ConnectionInfo.Id} left group {groupId}");
//    }

//    public IEnumerable<IWebSocketConnection> GetSocketsInGroup(string groupId)
//    {
//        lock (_groupLock)
//        {
//            if (_groups.TryGetValue(groupId, out var groupList))
//            {
//                // Return a copy
//                return new List<IWebSocketConnection>(groupList);
//            }
//        }
//        return Enumerable.Empty<IWebSocketConnection>();
//    }
//}