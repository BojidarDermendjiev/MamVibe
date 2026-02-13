namespace MomVibe.WebApi.Hubs;

using System.Security.Claims;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.Messages;
using Infrastructure.Services;

/// <summary>
/// Authenticated SignalR chat hub providing real-time messaging features:
/// - Send messages and receive them in real time
/// - Mark messages as read (read receipts)
/// - Broadcast typing indicators
/// - Track user presence (online/offline) using per-user groups (<c>user_{userId}</c>)
/// User presence is tracked via the injected <see cref="UserPresenceTracker"/> singleton.
/// </summary>

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IMessageService _messageService;
    private readonly UserPresenceTracker _presenceTracker;

    public ChatHub(IMessageService messageService, UserPresenceTracker presenceTracker)
    {
        this._messageService = messageService;
        this._presenceTracker = presenceTracker;
    }

    public async Task<MessageDto> SendMessage(string receiverId, string content)
    {
        var senderId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var message = await this._messageService.SendMessageAsync(senderId, receiverId, content);

        await Clients.Group($"user_{receiverId}").ReceiveMessage(message);
        return message;
    }

    public async Task MarkAsRead(string senderId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await this._messageService.MarkAsReadAsync(userId, senderId);
        await Clients.Group($"user_{senderId}").MessageRead(userId);
    }

    public async Task SendTyping(string receiverId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await Clients.Group($"user_{receiverId}").UserTyping(userId);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        this._presenceTracker.AddConnection(userId, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await Clients.Others.UserOnline(userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        var wentOffline = this._presenceTracker.RemoveConnection(userId, Context.ConnectionId);
        if (wentOffline)
        {
            await Clients.Others.UserOffline(userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Strongly-typed SignalR client contract for chat events.
/// </summary>
public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);
    Task MessageRead(string readByUserId);
    Task UserTyping(string userId);
    Task UserOnline(string userId);
    Task UserOffline(string userId);
}
