using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

using MomVibe.Application.DTOs.Messages;
using MomVibe.Application.Interfaces;
using MomVibe.Infrastructure.Services;
using MomVibe.WebApi.Hubs;

namespace MomVibe.UnitTests.Hubs;

/// <summary>
/// Unit tests for ChatHub methods using mocked SignalR infrastructure.
/// Tests hub method validation, service delegation, and group routing.
/// UserPresenceTracker is backed by MemoryDistributedCache — no Redis needed.
/// </summary>
public class ChatHubTests
{
    private const string SenderId = "sender-001";
    private const string ReceiverId = "receiver-002";
    private const string ConnectionId = "test-connection-id";

    private readonly Mock<IMessageService> _messageService = new();
    private readonly Mock<IHubCallerClients<IChatClient>> _clients = new();
    private readonly Mock<IChatClient> _callerClient = new();
    private readonly Mock<IChatClient> _groupClient = new();
    private readonly Mock<IGroupManager> _groups = new();
    private readonly Mock<HubCallerContext> _context = new();

    private static IDistributedCache CreateCache() =>
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    private readonly UserPresenceTracker _presenceTracker = new(CreateCache());

    private ChatHub BuildHub()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, SenderId)], "test"));

        _context.Setup(c => c.User).Returns(principal);
        _context.Setup(c => c.ConnectionId).Returns(ConnectionId);

        // Route group(user_X) calls to the group client mock
        _clients.Setup(c => c.Group(It.IsAny<string>())).Returns(_groupClient.Object);
        _clients.Setup(c => c.Others).Returns(_groupClient.Object);

        _groups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        _groups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        _groupClient.Setup(c => c.ReceiveMessage(It.IsAny<MessageDto>())).Returns(Task.CompletedTask);
        _groupClient.Setup(c => c.MessageRead(It.IsAny<string>())).Returns(Task.CompletedTask);
        _groupClient.Setup(c => c.UserTyping(It.IsAny<string>())).Returns(Task.CompletedTask);
        _groupClient.Setup(c => c.UserOnline(It.IsAny<string>())).Returns(Task.CompletedTask);
        _groupClient.Setup(c => c.UserOffline(It.IsAny<string>())).Returns(Task.CompletedTask);

        return new ChatHub(_messageService.Object, _presenceTracker, NullLogger<ChatHub>.Instance)
        {
            Context = _context.Object,
            Clients = _clients.Object,
            Groups = _groups.Object,
        };
    }

    // ── SendMessage validation ────────────────────────────────────────────────

    [Fact]
    public async Task SendMessage_EmptyReceiverId_ThrowsHubException()
    {
        var hub = BuildHub();
        var act = () => hub.SendMessage("", "Hello");
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*ReceiverId is required*");
    }

    [Fact]
    public async Task SendMessage_WhitespaceReceiverId_ThrowsHubException()
    {
        var hub = BuildHub();
        var act = () => hub.SendMessage("   ", "Hello");
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*ReceiverId is required*");
    }

    [Fact]
    public async Task SendMessage_EmptyContent_ThrowsHubException()
    {
        var hub = BuildHub();
        var act = () => hub.SendMessage(ReceiverId, "");
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public async Task SendMessage_ContentOver2000Chars_ThrowsHubException()
    {
        var hub = BuildHub();
        var act = () => hub.SendMessage(ReceiverId, new string('x', 2001));
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*2000*");
    }

    [Fact]
    public async Task SendMessage_ToSelf_ThrowsHubException()
    {
        var hub = BuildHub();
        var act = () => hub.SendMessage(SenderId, "Hi me");
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("*yourself*");
    }

    [Fact]
    public async Task SendMessage_ValidArgs_CallsMessageServiceAndReturnsDto()
    {
        var hub = BuildHub();
        var expected = new MessageDto
        {
            Id = Guid.NewGuid(),
            SenderId = SenderId,
            ReceiverId = ReceiverId,
            Content = "Hello!",
            Timestamp = DateTime.UtcNow,
        };
        _messageService.Setup(s => s.SendMessageAsync(SenderId, ReceiverId, "Hello!"))
                       .ReturnsAsync(expected);

        var result = await hub.SendMessage(ReceiverId, "Hello!");

        result.Should().Be(expected);
        _messageService.Verify(s => s.SendMessageAsync(SenderId, ReceiverId, "Hello!"), Times.Once);
    }

    [Fact]
    public async Task SendMessage_ValidArgs_PushesMessageToReceiverGroup()
    {
        var hub = BuildHub();
        var dto = new MessageDto { Id = Guid.NewGuid(), SenderId = SenderId, ReceiverId = ReceiverId, Content = "Hi" };
        _messageService.Setup(s => s.SendMessageAsync(SenderId, ReceiverId, "Hi")).ReturnsAsync(dto);

        await hub.SendMessage(ReceiverId, "Hi");

        _clients.Verify(c => c.Group($"user_{ReceiverId}"), Times.AtLeastOnce);
        _groupClient.Verify(c => c.ReceiveMessage(dto), Times.Once);
    }

    // ── MarkAsRead ────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsRead_CallsServiceAndNotifiesSenderGroup()
    {
        var hub = BuildHub();
        _messageService.Setup(s => s.MarkAsReadAsync(SenderId, "other-user")).Returns(Task.CompletedTask);

        await hub.MarkAsRead("other-user");

        _messageService.Verify(s => s.MarkAsReadAsync(SenderId, "other-user"), Times.Once);
        _clients.Verify(c => c.Group("user_other-user"), Times.Once);
        _groupClient.Verify(c => c.MessageRead(SenderId), Times.Once);
    }

    // ── SendTyping ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendTyping_BroadcastsTypingToReceiverGroup()
    {
        var hub = BuildHub();
        await hub.SendTyping(ReceiverId);

        _clients.Verify(c => c.Group($"user_{ReceiverId}"), Times.Once);
        _groupClient.Verify(c => c.UserTyping(SenderId), Times.Once);
    }

    // ── OnConnectedAsync / OnDisconnectedAsync ────────────────────────────────

    [Fact]
    public async Task OnConnectedAsync_AddsConnectionToPresenceTracker()
    {
        var hub = BuildHub();
        await hub.OnConnectedAsync();

        (await _presenceTracker.IsOnlineAsync(SenderId)).Should().BeTrue();
    }

    [Fact]
    public async Task OnConnectedAsync_JoinsUserGroup()
    {
        var hub = BuildHub();
        await hub.OnConnectedAsync();

        _groups.Verify(g => g.AddToGroupAsync(ConnectionId, $"user_{SenderId}", default), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_RemovesConnectionFromPresenceTracker()
    {
        var hub = BuildHub();
        await hub.OnConnectedAsync();
        await hub.OnDisconnectedAsync(null);

        (await _presenceTracker.IsOnlineAsync(SenderId)).Should().BeFalse();
    }
}
