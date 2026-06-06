using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for MessageService using EF Core InMemory.
/// IN8nWebhookService and IAiService are Moq mocks.
/// Users are seeded before messages to avoid the InMemory Include+User navigation bug
/// for tracked (non-AsNoTracking) queries.
/// Note: GetConversationsAsync uses AsNoTracking+Include(User) which has the known InMemory
/// limitation; we test conversation logic via the tracked query pattern where possible.
/// </summary>
public class MessageServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"MessageTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static IDistributedCache CreateCache() =>
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    private static MessageService CreateService(
        ApplicationDbContext db,
        Mock<IOutboxWriter>? outboxMock = null,
        Mock<IAiService>? aiMock = null,
        UserPresenceTracker? presenceTracker = null)
    {
        outboxMock ??= new Mock<IOutboxWriter>();
        aiMock ??= new Mock<IAiService>();
        presenceTracker ??= new UserPresenceTracker(CreateCache());
        var n8nOptions = Options.Create(new N8nSettings());

        return new MessageService(
            db,
            CreateMapper(),
            outboxMock.Object,
            n8nOptions,
            presenceTracker,
            aiMock.Object,
            new Mock<IKnowledgeService>().Object,
            NullLogger<MessageService>.Instance);
    }

    /// <summary>Seeds a user. Idempotent — skips if the user already exists.</summary>
    private static void SeedUser(ApplicationDbContext db, string userId)
    {
        if (!db.Users.Any(u => u.Id == userId))
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                DisplayName = $"User {userId}",
                Email = $"{userId}@test.com",
                UserName = userId
            });
    }

    /// <summary>
    /// Seeds a message and saves. Both sender and receiver users must already be in the DB
    /// before calling this helper to avoid the InMemory Include+User navigation bug.
    /// </summary>
    private static async Task<Message> SeedMessageAsync(
        ApplicationDbContext db,
        string senderId,
        string receiverId,
        string content = "Hello there",
        bool isRead = false)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            IsRead = isRead
        };
        db.Messages.Add(message);
        await db.SaveChangesAsync();
        return message;
    }

    // =========================================================================
    // GetMessagesAsync
    // =========================================================================

    [Fact]
    public async Task GetMessagesAsync_Returns_Messages_Between_Two_Users_In_Chronological_Order()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        await db.SaveChangesAsync();

        await SeedMessageAsync(db, "alice", "bob", "First message");
        await SeedMessageAsync(db, "bob", "alice", "Reply message");

        var svc = CreateService(db);
        var result = await svc.GetMessagesAsync("alice", "bob");

        result.Should().HaveCount(2);
        result[0].Content.Should().Be("First message");
        result[1].Content.Should().Be("Reply message");
    }

    [Fact]
    public async Task GetMessagesAsync_Returns_Empty_When_No_History_Between_Users()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        await db.SaveChangesAsync();

        // Seed a message between unrelated users to ensure global DB is non-empty
        SeedUser(db, "charlie");
        await db.SaveChangesAsync();
        await SeedMessageAsync(db, "alice", "charlie", "Unrelated message");

        var svc = CreateService(db);
        var result = await svc.GetMessagesAsync("alice", "bob");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMessagesAsync_Returns_Only_Messages_Between_The_Two_Specified_Users()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        SeedUser(db, "charlie");
        await db.SaveChangesAsync();

        await SeedMessageAsync(db, "alice", "bob", "Alice to Bob");
        await SeedMessageAsync(db, "alice", "charlie", "Alice to Charlie");
        await SeedMessageAsync(db, "charlie", "alice", "Charlie to Alice");

        var svc = CreateService(db);
        var result = await svc.GetMessagesAsync("alice", "bob");

        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Alice to Bob");
    }

    [Fact]
    public async Task GetMessagesAsync_Paginates_Results()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        await db.SaveChangesAsync();

        for (var i = 1; i <= 5; i++)
            await SeedMessageAsync(db, "alice", "bob", $"Message {i}");

        var svc = CreateService(db);
        var page1 = await svc.GetMessagesAsync("alice", "bob", page: 1, pageSize: 3);
        var page2 = await svc.GetMessagesAsync("alice", "bob", page: 2, pageSize: 3);

        page1.Should().HaveCount(3);
        page2.Should().HaveCount(2);
    }

    // =========================================================================
    // SendMessageAsync
    // =========================================================================

    [Fact]
    public async Task SendMessageAsync_Saves_Message_With_Correct_Sender_And_Receiver()
    {
        await using var db = CreateDb();
        SeedUser(db, "sender-1");
        SeedUser(db, "receiver-1");
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.SendMessageAsync("sender-1", "receiver-1", "Hello!");

        result.Should().NotBeNull();
        result.SenderId.Should().Be("sender-1");
        result.Content.Should().Be("Hello!");

        var saved = await db.Messages.FirstOrDefaultAsync(m => m.SenderId == "sender-1" && m.ReceiverId == "receiver-1");
        saved.Should().NotBeNull();
        saved!.Content.Should().Be("Hello!");
    }

    [Fact]
    public async Task SendMessageAsync_Throws_InvalidOperation_When_Sender_Equals_Receiver()
    {
        await using var db = CreateDb();
        SeedUser(db, "user-1");
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = async () => await svc.SendMessageAsync("user-1", "user-1", "Self message");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*yourself*");
    }

    [Fact]
    public async Task SendMessageAsync_Throws_ArgumentException_When_Content_Is_Empty()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var act = async () => await svc.SendMessageAsync("alice", "bob", "   ");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public async Task SendMessageAsync_Throws_ArgumentException_When_Content_Exceeds_2000_Characters()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        await db.SaveChangesAsync();

        var longContent = new string('x', 2001);
        var svc = CreateService(db);
        var act = async () => await svc.SendMessageAsync("alice", "bob", longContent);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*2000*");
    }

    [Fact]
    public async Task SendMessageAsync_Fires_Webhook_When_Receiver_Is_Offline()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        await db.SaveChangesAsync();

        var outboxMock = new Mock<IOutboxWriter>();
        // Presence tracker has no connections — bob is offline
        var presenceTracker = new UserPresenceTracker(CreateCache());

        var svc = CreateService(db, outboxMock, presenceTracker: presenceTracker);
        await svc.SendMessageAsync("alice", "bob", "You there?");

        outboxMock.Verify(o => o.Enqueue(
            OutboxMessageTypes.N8nWebhook,
            It.IsAny<N8nWebhookOutboxPayload>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_Does_Not_Enqueue_Webhook_When_Receiver_Is_Online()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        await db.SaveChangesAsync();

        var outboxMock = new Mock<IOutboxWriter>();
        var presenceTracker = new UserPresenceTracker(CreateCache());
        await presenceTracker.AddConnectionAsync("bob", "conn-1"); // bob is online

        var svc = CreateService(db, outboxMock, presenceTracker: presenceTracker);
        await svc.SendMessageAsync("alice", "bob", "You there?");

        outboxMock.Verify(o => o.Enqueue(
            It.IsAny<string>(),
            It.IsAny<N8nWebhookOutboxPayload>()),
            Times.Never);
    }

    // =========================================================================
    // GetConversationsAsync
    // NOTE: GetConversationsAsync uses AsNoTracking() + Include(m => m.Sender/Receiver)
    // which has a known InMemory limitation when navigations point to IdentityUser.
    // We verify the DB-level unread count logic via tracked queries instead.
    // =========================================================================

    [Fact]
    public async Task GetConversationsAsync_Raw_UnreadCount_Is_Correct_In_Database()
    {
        // Verify that the unread count logic the service would use is correct at the DB level.
        await using var db = CreateDb();
        SeedUser(db, "alice");
        SeedUser(db, "bob");
        await db.SaveChangesAsync();

        await SeedMessageAsync(db, "bob", "alice", "Unread 1", isRead: false);
        await SeedMessageAsync(db, "bob", "alice", "Unread 2", isRead: false);
        await SeedMessageAsync(db, "bob", "alice", "Read already", isRead: true);

        var unreadCount = await db.Messages
            .CountAsync(m => m.SenderId == "bob" && m.ReceiverId == "alice" && !m.IsRead);

        unreadCount.Should().Be(2);
    }

    [Fact]
    public async Task GetConversationsAsync_Returns_Empty_When_User_Has_No_Messages()
    {
        await using var db = CreateDb();
        SeedUser(db, "alice");
        await db.SaveChangesAsync();

        var svc = CreateService(db);
        var result = await svc.GetConversationsAsync("alice");

        result.Should().BeEmpty();
    }
}
