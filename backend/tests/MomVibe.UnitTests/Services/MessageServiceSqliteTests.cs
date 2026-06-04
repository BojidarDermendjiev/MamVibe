using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;
using AutoMapper;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// SQLite-backed tests for MessageService methods that use ExecuteUpdateAsync,
/// which is not supported by the EF Core InMemory provider.
/// </summary>
public class MessageServiceSqliteTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ApplicationDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new SqliteTestDbContext(options);
        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private MessageService CreateService()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        var mapper = cfg.CreateMapper();
        return new MessageService(
            _db,
            mapper,
            new Mock<IOutboxWriter>().Object,
            Options.Create(new N8nSettings()),
            new UserPresenceTracker(),
            new Mock<IAiService>().Object,
            new Mock<IKnowledgeService>().Object,
            NullLogger<MessageService>.Instance);
    }

    private ApplicationUser SeedUser(string userId)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@test.com",
            NormalizedUserName = $"{userId}@test.com".ToUpperInvariant(),
            Email = $"{userId}@test.com",
            NormalizedEmail = $"{userId}@test.com".ToUpperInvariant(),
            DisplayName = $"User {userId}",
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
        };
        _db.Users.Add(user);
        return user;
    }

    private async Task<Message> SeedMessageAsync(string senderId, string receiverId, string content, bool isRead)
    {
        var msg = new Message { SenderId = senderId, ReceiverId = receiverId, Content = content, IsRead = isRead };
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();
        return msg;
    }

    [Fact]
    public async Task MarkAsReadAsync_Marks_Only_Unread_Messages_From_Specified_Sender()
    {
        SeedUser("alice");
        SeedUser("bob");
        SeedUser("charlie");
        await _db.SaveChangesAsync();

        await SeedMessageAsync("bob", "alice", "Message 1", isRead: false);
        await SeedMessageAsync("bob", "alice", "Message 2", isRead: false);
        await SeedMessageAsync("charlie", "alice", "Charlie message", isRead: false);

        await CreateService().MarkAsReadAsync("alice", "bob");

        // ExecuteUpdate bypasses the change tracker — clear it so re-queries hit the DB
        _db.ChangeTracker.Clear();

        var bobMessages = await _db.Messages
            .Where(m => m.SenderId == "bob" && m.ReceiverId == "alice")
            .ToListAsync();
        bobMessages.Should().AllSatisfy(m => m.IsRead.Should().BeTrue());

        var charlieMessage = await _db.Messages
            .FirstAsync(m => m.SenderId == "charlie" && m.ReceiverId == "alice");
        charlieMessage.IsRead.Should().BeFalse("Charlie's message should not be affected");
    }

    [Fact]
    public async Task MarkAsReadAsync_Does_Not_Mark_Already_Read_Messages_As_Unread()
    {
        SeedUser("alice");
        SeedUser("bob");
        await _db.SaveChangesAsync();

        var msg = await SeedMessageAsync("bob", "alice", "Already read", isRead: true);

        await CreateService().MarkAsReadAsync("alice", "bob");

        var fetched = await _db.Messages.FindAsync(msg.Id);
        fetched!.IsRead.Should().BeTrue();
    }
}
