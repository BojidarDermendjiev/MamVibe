using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for AuditLogService using EF Core InMemory.
/// AuditLogService is append-only — only LogAsync (write) is tested; reads are done via
/// the DbContext directly to assert persisted state.
/// </summary>
public class AuditLogServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"AuditLogTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static AuditLogService CreateService(ApplicationDbContext db) =>
        new AuditLogService(db);

    // =========================================================================
    // LogAsync
    // =========================================================================

    [Fact]
    public async Task LogAsync_Saves_Record_With_Correct_UserId_And_Action()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        await svc.LogAsync("user-1", "Auth.Login", success: true);

        var log = await db.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.UserId.Should().Be("user-1");
        log.Action.Should().Be("Auth.Login");
        log.Success.Should().BeTrue();
    }

    [Fact]
    public async Task LogAsync_Persists_Optional_Fields_When_Provided()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        await svc.LogAsync(
            "user-2",
            "Admin.BlockUser",
            success: true,
            targetId: "target-42",
            ipAddress: "192.168.1.1",
            details: "Blocked for spam");

        var log = await db.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.TargetId.Should().Be("target-42");
        log.IpAddress.Should().Be("192.168.1.1");
        log.Details.Should().Be("Blocked for spam");
    }

    [Fact]
    public async Task LogAsync_Stores_Failure_Flag_Correctly()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        await svc.LogAsync("user-3", "Auth.Login", success: false, details: "Wrong password");

        var log = await db.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task LogAsync_Sets_CreatedAt_Timestamp_Automatically()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);
        var before = DateTime.UtcNow;

        await svc.LogAsync("user-1", "Auth.Login", success: true);

        var log = await db.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.CreatedAt.Should().BeOnOrAfter(before);
        log.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task LogAsync_Appends_Multiple_Independent_Records()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        await svc.LogAsync("user-1", "Auth.Login", success: true);
        await svc.LogAsync("user-2", "Admin.BlockUser", success: true, targetId: "user-1");
        await svc.LogAsync("user-1", "Payment.Complete", success: true, targetId: "pay-99");

        var count = await db.AuditLogs.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task LogAsync_Leaves_OptionalFields_Null_When_Not_Provided()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        await svc.LogAsync("user-1", "Auth.Logout", success: true);

        var log = await db.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.TargetId.Should().BeNull();
        log.IpAddress.Should().BeNull();
        log.Details.Should().BeNull();
    }

    [Fact]
    public async Task LogAsync_Each_Record_Gets_Unique_Id()
    {
        await using var db = CreateDb();
        var svc = CreateService(db);

        await svc.LogAsync("user-1", "Auth.Login", success: true);
        await svc.LogAsync("user-1", "Auth.Login", success: true);

        var ids = await db.AuditLogs.Select(l => l.Id).ToListAsync();
        ids.Should().HaveCount(2);
        ids[0].Should().NotBe(ids[1]);
    }
}
