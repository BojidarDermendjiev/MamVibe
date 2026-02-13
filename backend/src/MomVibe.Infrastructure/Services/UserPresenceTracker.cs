namespace MomVibe.Infrastructure.Services;

using System.Collections.Concurrent;

/// <summary>
/// Thread-safe singleton tracker for SignalR user presence.
/// Replaces the static dictionary previously held in ChatHub.
/// </summary>
public class UserPresenceTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();

    public void AddConnection(string userId, string connectionId)
    {
        _connections.AddOrUpdate(userId,
            _ => new HashSet<string> { connectionId },
            (_, set) => { lock (set) { set.Add(connectionId); } return set; });
    }

    public bool RemoveConnection(string userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var connections))
            return false;

        lock (connections)
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                _connections.TryRemove(userId, out _);
                return true; // last connection removed — user went offline
            }
        }

        return false;
    }

    public bool IsOnline(string userId) => _connections.ContainsKey(userId);
}
