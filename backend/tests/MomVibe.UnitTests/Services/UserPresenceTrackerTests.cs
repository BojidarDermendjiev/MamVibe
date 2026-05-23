using FluentAssertions;

using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for UserPresenceTracker — an in-memory, thread-safe SignalR connection registry.
/// No database or mocks required.
/// </summary>
public class UserPresenceTrackerTests
{
    // =========================================================================
    // AddConnection / IsOnline
    // =========================================================================

    [Fact]
    public void AddConnection_Makes_User_Online()
    {
        var tracker = new UserPresenceTracker();

        tracker.AddConnection("user-1", "conn-a");

        tracker.IsOnline("user-1").Should().BeTrue();
    }

    [Fact]
    public void IsOnline_Returns_False_For_Unknown_User()
    {
        var tracker = new UserPresenceTracker();

        tracker.IsOnline("ghost-user").Should().BeFalse();
    }

    // =========================================================================
    // RemoveConnection
    // =========================================================================

    [Fact]
    public void RemoveConnection_Makes_User_Offline_After_Last_Connection_Removed()
    {
        var tracker = new UserPresenceTracker();
        tracker.AddConnection("user-1", "conn-a");

        var wentOffline = tracker.RemoveConnection("user-1", "conn-a");

        wentOffline.Should().BeTrue();
        tracker.IsOnline("user-1").Should().BeFalse();
    }

    [Fact]
    public void RemoveConnection_Returns_False_When_User_Still_Has_Other_Connections()
    {
        var tracker = new UserPresenceTracker();
        tracker.AddConnection("user-1", "conn-a");
        tracker.AddConnection("user-1", "conn-b");

        var wentOffline = tracker.RemoveConnection("user-1", "conn-a");

        wentOffline.Should().BeFalse();
        tracker.IsOnline("user-1").Should().BeTrue();
    }

    [Fact]
    public void RemoveConnection_Returns_False_For_Completely_Unknown_User()
    {
        var tracker = new UserPresenceTracker();

        var result = tracker.RemoveConnection("never-connected", "conn-x");

        result.Should().BeFalse();
    }

    // =========================================================================
    // Multiple connections from the same user
    // =========================================================================

    [Fact]
    public void User_Remains_Online_Until_All_Connections_Removed()
    {
        var tracker = new UserPresenceTracker();
        tracker.AddConnection("user-1", "conn-a");
        tracker.AddConnection("user-1", "conn-b");
        tracker.AddConnection("user-1", "conn-c");

        tracker.RemoveConnection("user-1", "conn-a");
        tracker.IsOnline("user-1").Should().BeTrue();

        tracker.RemoveConnection("user-1", "conn-b");
        tracker.IsOnline("user-1").Should().BeTrue();

        var wentOffline = tracker.RemoveConnection("user-1", "conn-c");
        wentOffline.Should().BeTrue();
        tracker.IsOnline("user-1").Should().BeFalse();
    }

    [Fact]
    public void AddConnection_Duplicate_ConnectionId_Does_Not_Create_Extra_Entry()
    {
        var tracker = new UserPresenceTracker();
        tracker.AddConnection("user-1", "conn-a");
        tracker.AddConnection("user-1", "conn-a"); // same connection added twice

        // Removing once should take the user offline because the set de-duplicates
        var wentOffline = tracker.RemoveConnection("user-1", "conn-a");
        wentOffline.Should().BeTrue();
        tracker.IsOnline("user-1").Should().BeFalse();
    }

    // =========================================================================
    // Multiple independent users
    // =========================================================================

    [Fact]
    public void Multiple_Users_Are_Tracked_Independently()
    {
        var tracker = new UserPresenceTracker();
        tracker.AddConnection("user-1", "conn-a");
        tracker.AddConnection("user-2", "conn-b");

        tracker.RemoveConnection("user-1", "conn-a");

        tracker.IsOnline("user-1").Should().BeFalse();
        tracker.IsOnline("user-2").Should().BeTrue();
    }
}
