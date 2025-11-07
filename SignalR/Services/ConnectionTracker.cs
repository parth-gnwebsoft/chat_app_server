using System.Collections.Concurrent;
using System.Text.Json.Serialization; // <-- 1. ADD THIS USING

namespace WebSocketTest.Services;

// ... (ConnectionTracker class is unchanged) ...
public class ConnectionTracker
{
    private readonly ConcurrentDictionary<string, ClientConnectionInfo> _connections = new();
    private readonly ConcurrentDictionary<string, string> _userConnections = new();

    public void Add(string connectionId, string userId, string token)
    {
        var connectionInfo = new ClientConnectionInfo
        {
            ConnectionId = connectionId,
            UserId = userId,
            AuthToken = token, // <-- Store the token
            ConnectedAt = DateTime.UtcNow
        };

        _connections[connectionId] = connectionInfo;
        _userConnections[userId] = connectionId;
    }

    public void Remove(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var info))
        {
            _userConnections.TryRemove(info.UserId, out _);
        }
    }

    public ClientConnectionInfo? GetConnectionByUserId(string userId)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            return GetConnection(connectionId);
        }
        return null;
    }

    public IEnumerable<ClientConnectionInfo> GetAllConnections()
    {
        return _connections.Values.ToList();
    }

    public ClientConnectionInfo? GetConnection(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var connection);
        return connection;
    }

    public int GetCount()
    {
        return _connections.Count;
    }
}


public class ClientConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }

    [JsonIgnore] // <-- Hide from client-side serialization
    public string AuthToken { get; set; } = string.Empty; // <-- NEW PROPERTY
    // --- 2. ADD THIS ATTRIBUTE ---
    [JsonIgnore]
    public string ConnectedFor => (DateTime.UtcNow - ConnectedAt).ToString(@"hh\:mm\:ss");
}