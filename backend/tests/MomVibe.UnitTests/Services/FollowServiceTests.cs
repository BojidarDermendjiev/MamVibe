using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

using MomVibe.Application.Interfaces;
using MomVibe.Domain.Entities;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

public class FollowServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"FollowTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static FollowService CreateService(ApplicationDbContext db, Mock<IFollowNotifier>? notifierMock = null)
    {
        notifierMock ??= new Mock<IFollowNotifier>();
        notifierMock.Setup(n => n.NotifyNewFollowerAsync(It.IsAny<string>(), It.IsAny<Application.DTOs.Follows.NewFollowerNotification>()))
                    .Returns(Task.CompletedTask);
        return new FollowService(db, notifierMock.Object, NullLogger<FollowService>.Instance);
    }

    private static async Task SeedUserAsync(ApplicationDbContext db, string userId, string displayName = "User")
    {
        if (!db.Users.Any(u => u.Id == userId))
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                DisplayName = displayName,
                Email = $"{userId}@test.com",
                UserName = userId
            });
            await db.SaveChangesAsync();
        }
    }

    // =========================================================================
    // ToggleFollowAsync — follow
    // =========================================================================

    [Fact]
    public async Task ToggleFollowAsync_Creates_Follow_And_Returns_IsFollowing_True()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "follower-1");
        await SeedUserAsync(db, "followee-1");

        var svc = CreateService(db);
        var result = await svc.ToggleFollowAsync("follower-1", "followee-1");

        result.IsFollowing.Should().BeTrue();
        result.FollowerCount.Should().Be(1);
        db.Follows.Should().HaveCount(1);
    }

    [Fact]
    public async Task ToggleFollowAsync_Removes_Follow_And_Returns_IsFollowing_False()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "follower-1");
        await SeedUserAsync(db, "followee-1");
        db.Follows.Add(new Follow { FollowerId = "follower-1", FolloweeId = "followee-1" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.ToggleFollowAsync("follower-1", "followee-1");

        result.IsFollowing.Should().BeFalse();
        result.FollowerCount.Should().Be(0);
        db.Follows.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleFollowAsync_Throws_InvalidOperationException_When_Following_Self()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "user-1");

        var svc = CreateService(db);
        var act = () => svc.ToggleFollowAsync("user-1", "user-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ToggleFollowAsync_Sends_NewFollower_Notification()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "follower-1", "Alice");
        await SeedUserAsync(db, "followee-1", "Bob");

        var notifierMock = new Mock<IFollowNotifier>();
        notifierMock.Setup(n => n.NotifyNewFollowerAsync(It.IsAny<string>(), It.IsAny<Application.DTOs.Follows.NewFollowerNotification>()))
                    .Returns(Task.CompletedTask);

        var svc = CreateService(db, notifierMock);
        await svc.ToggleFollowAsync("follower-1", "followee-1");

        notifierMock.Verify(
            n => n.NotifyNewFollowerAsync("followee-1", It.Is<Application.DTOs.Follows.NewFollowerNotification>(
                notif => notif.FollowerId == "follower-1")),
            Times.Once);
    }

    [Fact]
    public async Task ToggleFollowAsync_Does_Not_Send_Notification_When_Unfollowing()
    {
        await using var db = CreateDb();
        await SeedUserAsync(db, "follower-1");
        await SeedUserAsync(db, "followee-1");
        db.Follows.Add(new Follow { FollowerId = "follower-1", FolloweeId = "followee-1" });
        await db.SaveChangesAsync();

        var notifierMock = new Mock<IFollowNotifier>();
        var svc = CreateService(db, notifierMock);
        await svc.ToggleFollowAsync("follower-1", "followee-1");

        notifierMock.Verify(
            n => n.NotifyNewFollowerAsync(It.IsAny<string>(), It.IsAny<Application.DTOs.Follows.NewFollowerNotification>()),
            Times.Never);
    }

    // =========================================================================
    // IsFollowingAsync
    // =========================================================================

    [Fact]
    public async Task IsFollowingAsync_Returns_True_When_Follow_Exists()
    {
        await using var db = CreateDb();
        db.Follows.Add(new Follow { FollowerId = "f1", FolloweeId = "f2" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.IsFollowingAsync("f1", "f2");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFollowingAsync_Returns_False_When_Follow_Does_Not_Exist()
    {
        await using var db = CreateDb();

        var svc = CreateService(db);
        var result = await svc.IsFollowingAsync("f1", "f2");

        result.Should().BeFalse();
    }

    // =========================================================================
    // GetFollowerCountAsync
    // =========================================================================

    [Fact]
    public async Task GetFollowerCountAsync_Returns_Correct_Count()
    {
        await using var db = CreateDb();
        db.Follows.Add(new Follow { FollowerId = "a", FolloweeId = "target" });
        db.Follows.Add(new Follow { FollowerId = "b", FolloweeId = "target" });
        db.Follows.Add(new Follow { FollowerId = "c", FolloweeId = "other" });
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var count = await svc.GetFollowerCountAsync("target");

        count.Should().Be(2);
    }

    // =========================================================================
    // GetFollowingFeedAsync
    // =========================================================================

    [Fact]
    public async Task GetFollowingFeedAsync_Returns_Empty_When_Not_Following_Anyone()
    {
        await using var db = CreateDb();

        var svc = CreateService(db);
        var result = await svc.GetFollowingFeedAsync("user-1", 1, 12);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
