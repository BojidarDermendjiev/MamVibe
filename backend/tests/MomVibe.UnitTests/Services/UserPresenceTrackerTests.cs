using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for UserPresenceTracker — a Redis-backed, thread-safe SignalR connection registry.
/// Uses MemoryDistributedCache (the in-memory IDistributedCache implementation) so no real
/// Redis instance is needed in the test environment.
/// </summary>
public class UserPresenceTrackerTests
{
    /// <summary>Creates a fresh in-memory IDistributedCache for each test.</summary>
    private static IDistributedCache CreateCache() =>
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    private static UserPresenceTracker CreateTracker() =>
        new UserPresenceTracker(CreateCache());

    // =========================================================================
    // AddConnectionAsync / IsOnlineAsync
    // =========================================================================

    [Fact]
    public async Task AddConnection_Makes_User_Online()
    {
        var tracker = CreateTracker();

        await tracker.AddConnectionAsync("user-1", "conn-a");

        (await tracker.IsOnlineAsync("user-1")).Should().BeTrue();
    }

    [Fact]
    public async Task IsOnline_Returns_False_For_Unknown_User()
    {
        var tracker = CreateTracker();

        (await tracker.IsOnlineAsync("ghost-user")).Should().BeFalse();
    }

    // =========================================================================
    // RemoveConnectionAsync
    // =========================================================================

    [Fact]
    public async Task RemoveConnection_Makes_User_Offline_After_Last_Connection_Removed()
    {
        var tracker = CreateTracker();
        await tracker.AddConnectionAsync("user-1", "conn-a");

        var wentOffline = await tracker.RemoveConnectionAsync("user-1", "conn-a");

        wentOffline.Should().BeTrue();
        (await tracker.IsOnlineAsync("user-1")).Should().BeFalse();
    }

    [Fact]
    public async Task RemoveConnection_Returns_False_When_User_Still_Has_Other_Connections()
    {
        var tracker = CreateTracker();
        await tracker.AddConnectionAsync("user-1", "conn-a");
        await tracker.AddConnectionAsync("user-1", "conn-b");

        var wentOffline = await tracker.RemoveConnectionAsync("user-1", "conn-a");

        wentOffline.Should().BeFalse();
        (await tracker.IsOnlineAsync("user-1")).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveConnection_Returns_False_For_Completely_Unknown_User()
    {
        var tracker = CreateTracker();

        var result = await tracker.RemoveConnectionAsync("never-connected", "conn-x");

        result.Should().BeFalse();
    }

    // =========================================================================
    // Multiple connections from the same user
    // =========================================================================

    [Fact]
    public async Task User_Remains_Online_Until_All_Connections_Removed()
    {
        var tracker = CreateTracker();
        await tracker.AddConnectionAsync("user-1", "conn-a");
        await tracker.AddConnectionAsync("user-1", "conn-b");
        await tracker.AddConnectionAsync("user-1", "conn-c");

        await tracker.RemoveConnectionAsync("user-1", "conn-a");
        (await tracker.IsOnlineAsync("user-1")).Should().BeTrue();

        await tracker.RemoveConnectionAsync("user-1", "conn-b");
        (await tracker.IsOnlineAsync("user-1")).Should().BeTrue();

        var wentOffline = await tracker.RemoveConnectionAsync("user-1", "conn-c");
        wentOffline.Should().BeTrue();
        (await tracker.IsOnlineAsync("user-1")).Should().BeFalse();
    }

    [Fact]
    public async Task AddConnection_Duplicate_ConnectionId_Increments_Counter()
    {
        // The counter-based approach counts each Add regardless of connection ID uniqueness.
        // Two Adds require two Removes to go offline.
        var tracker = CreateTracker();
        await tracker.AddConnectionAsync("user-1", "conn-a");
        await tracker.AddConnectionAsync("user-1", "conn-a"); // same connection added twice

        // First remove: still online (counter was 2, now 1)
        var firstRemove = await tracker.RemoveConnectionAsync("user-1", "conn-a");
        firstRemove.Should().BeFalse();
        (await tracker.IsOnlineAsync("user-1")).Should().BeTrue();

        // Second remove: now offline (counter was 1, now 0)
        var secondRemove = await tracker.RemoveConnectionAsync("user-1", "conn-a");
        secondRemove.Should().BeTrue();
        (await tracker.IsOnlineAsync("user-1")).Should().BeFalse();
    }

    // =========================================================================
    // Multiple independent users
    // =========================================================================

    [Fact]
    public async Task Multiple_Users_Are_Tracked_Independently()
    {
        var tracker = CreateTracker();
        await tracker.AddConnectionAsync("user-1", "conn-a");
        await tracker.AddConnectionAsync("user-2", "conn-b");

        await tracker.RemoveConnectionAsync("user-1", "conn-a");

        (await tracker.IsOnlineAsync("user-1")).Should().BeFalse();
        (await tracker.IsOnlineAsync("user-2")).Should().BeTrue();
    }

    // =========================================================================
    // Synchronous shim
    // =========================================================================

    [Fact]
    public async Task IsOnline_Sync_Shim_Returns_Correct_Value()
    {
        var tracker = CreateTracker();
        await tracker.AddConnectionAsync("user-1", "conn-a");

        tracker.IsOnline("user-1").Should().BeTrue();
        tracker.IsOnline("ghost").Should().BeFalse();
    }
}
