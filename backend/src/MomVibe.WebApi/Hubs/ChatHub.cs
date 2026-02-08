namespace MomVibe.WebApi.Hubs;

using System.Security.Claims;
using System.Collections.Concurrent;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.Messages;

/// <summary>
/// Authenticated SignalR chat hub providing real-time messaging features:
/// - Send messages and receive them in real time
/// - Mark messages as read (read receipts)
/// - Broadcast typing indicators
/// - Track user presence (online/offline) using per-user groups (<c>user_{userId}</c>)
/// A static in-memory connection map is maintained to track multiple connections per user.
/// </summary>

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();
    private readonly IMessageService _messageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatHub"/>.
    /// </summary>
    /// <param name="messageService">Service responsible for persisting and retrieving messages.</param>
    public ChatHub(IMessageService messageService)
    {
        this._messageService = messageService;
    }

    /// <summary>
    /// Sends a message from the authenticated user to the specified receiver,
    /// persists it, broadcasts to the receiver's group, and returns the message to the caller.
    /// </summary>
    /// <param name="receiverId">The identifier of the message recipient.</param>
    /// <param name="content">The text content of the message.</param>
    /// <returns>The persisted message DTO.</returns>
    public async Task<MessageDto> SendMessage(string receiverId, string content)
    {
        var senderId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var message = await this._messageService.SendMessageAsync(senderId, receiverId, content);

        await Clients.Group($"user_{receiverId}").ReceiveMessage(message);
        return message;
    }


    /// <summary>
    /// Marks messages from the specified sender as read for the authenticated user,
    /// and notifies the sender's group with a read receipt.
    /// </summary>
    /// <param name="senderId">The identifier of the sender whose messages should be marked as read.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task MarkAsRead(string senderId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await this._messageService.MarkAsReadAsync(userId, senderId);
        await Clients.Group($"user_{senderId}").MessageRead(userId);
    }

    /// <summary>
    /// Broadcasts a typing indicator to the receiver's group for the authenticated user.
    /// </summary>
    /// <param name="receiverId">The identifier of the user to notify about typing.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendTyping(string receiverId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await Clients.Group($"user_{receiverId}").UserTyping(userId);
    }

    /// <summary>
    /// Handles a new connection:
    /// - Adds the connection to the per-user map
    /// - Joins the connection to the user's group (<c>user_{userId}</c>)
    /// - Notifies other clients that the user is online
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        _connections.AddOrUpdate(userId,
            _ => new HashSet<string> { Context.ConnectionId },
            (_, set) => { set.Add(Context.ConnectionId); return set; });

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await Clients.Others.UserOnline(userId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handles a disconnection:
    /// - Removes the connection from the per-user map
    /// - If no connections remain for the user, marks them offline and notifies others
    /// </summary>
    /// <param name="exception">An optional exception associated with the disconnect.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (_connections.TryGetValue(userId, out var connections))
        {
            connections.Remove(Context.ConnectionId);
            if (connections.Count == 0)
            {
                _connections.TryRemove(userId, out _);
                await Clients.Others.UserOffline(userId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Indicates whether the specified user currently has any active hub connections.
    /// </summary>
    /// <param name="userId">The identifier of the user to check.</param>
    /// <returns><c>true</c> if the user has active connections; otherwise <c>false</c>.</returns>
    public static bool IsUserOnline(string userId) => _connections.ContainsKey(userId);
}

/// <summary>
/// Strongly-typed SignalR client contract for chat events.
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// Receives a newly created or incoming message.
    /// </summary>
    /// <param name="message">The message payload.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReceiveMessage(MessageDto message);

    /// <summary>
    /// Notifies that messages from a conversation peer were marked as read.
    /// </summary>
    /// <param name="readByUserId">The identifier of the user who read the messages.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MessageRead(string readByUserId);

    /// <summary>
    /// Indicates that a user is currently typing.
    /// </summary>
    /// <param name="userId">The identifier of the typing user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UserTyping(string userId);

    /// <summary>
    /// Notifies clients that a user came online.
    /// </summary>
    /// <param name="userId">The identifier of the online user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UserOnline(string userId);

    /// <summary>
    /// Notifies clients that a user went offline.
    /// </summary>
    /// <param name="userId">The identifier of the offline user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UserOffline(string userId);
}
