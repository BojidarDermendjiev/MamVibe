using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using MomVibe.Application.Interfaces;
using MomVibe.Domain.Entities;
using MomVibe.Infrastructure.Outbox;
using MomVibe.Infrastructure.Persistence;

namespace MomVibe.UnitTests.Outbox;

/// <summary>
/// Pinning tests for <see cref="OutboxWriter"/>. It must stage the row on the shared DbContext
/// without calling SaveChanges — the caller commits the outbox alongside the originating
/// business state change in a single transaction.
/// </summary>
public class OutboxWriterTests
{
    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"OutboxWriter_{Guid.NewGuid()}")
            .Options);

    private sealed record SamplePayload(string Path, string Body);

    [Fact]
    public void Enqueue_Adds_OutboxMessage_To_ChangeTracker_Without_Saving()
    {
        using var db = CreateDb();
        var writer = new OutboxWriter(db);

        writer.Enqueue("N8nWebhook", new SamplePayload("payment-completed", "{}"));

        // Added to change tracker but not persisted yet.
        db.ChangeTracker.Entries<OutboxMessage>().Should().HaveCount(1);
        db.OutboxMessages.Count().Should().Be(0);
    }

    [Fact]
    public async Task Enqueue_Then_SaveChanges_Persists_With_Serialized_Payload()
    {
        await using var db = CreateDb();
        var writer = new OutboxWriter(db);

        writer.Enqueue("N8nWebhook", new SamplePayload("payment-completed", "{\"id\":42}"));
        await db.SaveChangesAsync();

        var row = await db.OutboxMessages.SingleAsync();
        row.MessageType.Should().Be("N8nWebhook");
        // camelCase JSON naming is part of the contract.
        row.Payload.Should().Contain("\"path\":\"payment-completed\"");
        row.Payload.Should().Contain("\"body\"");
        row.ProcessedAt.Should().BeNull();
        row.AttemptCount.Should().Be(0);
        row.NextAttemptAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Enqueue_Stages_Multiple_Messages_In_One_Unit_Of_Work()
    {
        using var db = CreateDb();
        var writer = new OutboxWriter(db);

        writer.Enqueue("N8nWebhook", new SamplePayload("a", "1"));
        writer.Enqueue("N8nWebhook", new SamplePayload("b", "2"));

        db.ChangeTracker.Entries<OutboxMessage>().Should().HaveCount(2);
    }
}
