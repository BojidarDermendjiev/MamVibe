namespace MomVibe.Infrastructure.Services;

using Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// Redis-backed user presence tracker for SignalR hubs.
/// Stores a connection-count string per user under the key <c>presence:{userId}</c>.
/// Incrementing on connect and decrementing on disconnect gives correct online/offline
/// semantics across multiple pods. A sliding TTL of 5 minutes ensures stale entries
/// (e.g. from pod crashes) are automatically evicted.
/// </summary>
public class UserPresenceTracker
{
    private readonly IDistributedCache _cache;

    private static readonly DistributedCacheEntryOptions PresenceOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    /// <summary>Initializes a new instance of <see cref="UserPresenceTracker"/> backed by <paramref name="cache"/>.</summary>
    public UserPresenceTracker(IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <summary>Registers a new SignalR connection for the specified user, incrementing their presence counter in Redis.</summary>
    /// <param name="userId">The identifier of the user who connected.</param>
    /// <param name="connectionId">The SignalR connection identifier (used for logging; the counter is per-user).</param>
    public async Task AddConnectionAsync(string userId, string connectionId)
    {
        var key   = PresenceKey(userId);
        var raw   = await _cache.GetStringAsync(key);
        var count = raw != null && int.TryParse(raw, out var n) ? n : 0;
        await _cache.SetStringAsync(key, (count + 1).ToString(), PresenceOptions);
    }

    /// <summary>
    /// Removes a SignalR connection for the specified user, decrementing their presence counter.
    /// When the counter reaches zero the key is removed and the method returns <c>true</c>.
    /// </summary>
    /// <param name="userId">The identifier of the user who disconnected.</param>
    /// <param name="connectionId">The SignalR connection identifier.</param>
    /// <returns><c>true</c> if this was the user's last connection (they went offline); otherwise <c>false</c>.</returns>
    public async Task<bool> RemoveConnectionAsync(string userId, string connectionId)
    {
        var key   = PresenceKey(userId);
        var raw   = await _cache.GetStringAsync(key);

        if (raw == null)
            return false;

        if (!int.TryParse(raw, out var count) || count <= 1)
        {
            await _cache.RemoveAsync(key);
            return true; // last connection removed — user is offline
        }

        await _cache.SetStringAsync(key, (count - 1).ToString(), PresenceOptions);
        return false;
    }

    /// <summary>Returns a value indicating whether the specified user currently has at least one active connection.</summary>
    /// <param name="userId">The identifier of the user to check.</param>
    public async Task<bool> IsOnlineAsync(string userId)
    {
        var raw = await _cache.GetStringAsync(PresenceKey(userId));
        return raw != null && int.TryParse(raw, out var n) && n > 0;
    }

    // ── Synchronous shims ────────────────────────────────────────────────────
    // Kept for callers (MessageService) that cannot easily be made async in the
    // current call chain. These run the async path synchronously via GetAwaiter().
    // Safe here because IDistributedCache implementations are non-blocking and
    // the callers are not on a ASP.NET Core synchronization context that could deadlock.

    /// <summary>Synchronous variant of <see cref="IsOnlineAsync"/>. Prefer the async overload where possible.</summary>
    public bool IsOnline(string userId) =>
        IsOnlineAsync(userId).GetAwaiter().GetResult();

    private static string PresenceKey(string userId) => $"presence:{userId}";
}
