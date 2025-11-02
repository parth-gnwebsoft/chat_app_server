// Services/WebSocketStateService.cs
using Fleck;
using System.Collections.Concurrent;

public class WebSocketStateService : IWebSocketStateService
{
    // This holds all connected clients
    private readonly List<IWebSocketConnection> _allSockets = new();

    // Maps <UserId, SocketConnection>
    private readonly ConcurrentDictionary<string, IWebSocketConnection> _onlineUsers = new();

    // Maps <GroupId, List_of_Sockets>
    private readonly ConcurrentDictionary<string, List<IWebSocketConnection>> _groups = new();

    private readonly object _socketLock = new();
    private readonly object _groupLock = new();

    public void AddSocket(IWebSocketConnection socket)
    {
        lock (_socketLock)
        {
            _allSockets.Add(socket);
        }
    }

    public string? GetUserId(IWebSocketConnection socket)
    {
        return _onlineUsers.FirstOrDefault(x => x.Value == socket).Key;
    }

    public bool RegisterUser(string userId, IWebSocketConnection socket)
    {
        _onlineUsers[userId] = socket; // Add or update
        return true;
    }

    public List<string> GetOnlineUserIds()
    {
        return _onlineUsers.Keys.ToList();
    }

    public IWebSocketConnection? GetSocketByUserId(string userId)
    {
        _onlineUsers.TryGetValue(userId, out var socket);
        return socket;
    }

    public List<IWebSocketConnection> GetAllSockets()
    {
        lock (_socketLock)
        {
            return new List<IWebSocketConnection>(_allSockets);
        }
    }

    public void RemoveSocket(IWebSocketConnection socket)
    {
        // Remove from main list
        lock (_socketLock)
        {
            _allSockets.Remove(socket);
        }

        // Find and remove from onlineUsers
        var userId = GetUserId(socket);
        if (userId != null)
        {
            _onlineUsers.TryRemove(userId, out _);
        }

        // Remove from all groups
        lock (_groupLock)
        {
            foreach (var group in _groups.Values)
            {
                group.Remove(socket);
            }
        }
    }

    public void JoinGroup(string groupId, IWebSocketConnection socket)
    {
        lock (_groupLock)
        {
            // Get or create the group list
            if (!_groups.ContainsKey(groupId))
            {
                _groups[groupId] = new List<IWebSocketConnection>();
            }
            // Add socket to group if not already in it
            if (!_groups[groupId].Contains(socket))
            {
                _groups[groupId].Add(socket);
            }
        }
    }

    public void CreateGroup(string groupId, IEnumerable<string> memberUserIds)
    {
        lock (_groupLock)
        {
            var groupList = _groups.GetOrAdd(groupId, new List<IWebSocketConnection>());
            foreach (var userId in memberUserIds)
            {
                if (GetSocketByUserId(userId) is { } memberSocket && !groupList.Contains(memberSocket))
                {
                    groupList.Add(memberSocket);
                }
            }
        }
    }

    public void LeaveGroup(string groupId, IWebSocketConnection socket)
    {
        lock (_groupLock)
        {
            if (_groups.TryGetValue(groupId, out var groupList))
            {
                groupList.Remove(socket);
            }
        }
    }

    public List<IWebSocketConnection> GetSocketsInGroup(string groupId)
    {
        lock (_groupLock)
        {
            if (_groups.TryGetValue(groupId, out var groupList))
            {
                return new List<IWebSocketConnection>(groupList); // Return a copy
            }
            return new List<IWebSocketConnection>();
        }
    }
}