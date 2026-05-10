namespace MomVibe.Infrastructure.Services;

using System.Collections.Concurrent;

/// <summary>
/// Thread-safe singleton tracker for SignalR user presence.
/// Replaces the static dictionary previously held in ChatHub.
/// </summary>
public class UserPresenceTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();

    /// <summary>Registers a new SignalR connection for the specified user.</summary>
    /// <param name="userId">The identifier of the user who connected.</param>
    /// <param name="connectionId">The SignalR connection identifier.</param>
    public void AddConnection(string userId, string connectionId)
    {
        _connections.AddOrUpdate(userId,
            _ => new HashSet<string> { connectionId },
            (_, set) => { lock (set) { set.Add(connectionId); } return set; });
    }

    /// <summary>Removes a SignalR connection for the specified user.</summary>
    /// <param name="userId">The identifier of the user who disconnected.</param>
    /// <param name="connectionId">The SignalR connection identifier to remove.</param>
    /// <returns><c>true</c> if this was the user's last connection (they went offline); otherwise <c>false</c>.</returns>
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

    /// <summary>Returns a value indicating whether the specified user currently has at least one active connection.</summary>
    /// <param name="userId">The identifier of the user to check.</param>
    public bool IsOnline(string userId) => _connections.ContainsKey(userId);
}
