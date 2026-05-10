namespace MomVibe.WebApi.Hubs;

using System.Security.Claims;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

using Application.Interfaces;
using Application.DTOs.Messages;
using Application.DTOs.PurchaseRequests;
using Application.DTOs.Shipping;
using Infrastructure.Services;
using Domain.Constants;

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

    /// <summary>Initializes a new instance of <see cref="ChatHub"/> with the message service and presence tracker.</summary>
    public ChatHub(IMessageService messageService, UserPresenceTracker presenceTracker)
    {
        this._messageService = messageService;
        this._presenceTracker = presenceTracker;
    }

    /// <summary>
    /// Sends a chat message from the current user to the specified receiver, persists it, and
    /// pushes it in real time. If the receiver is the AI bot, also streams the bot reply.
    /// </summary>
    /// <param name="receiverId">The identifier of the message recipient.</param>
    /// <param name="content">The message text (max 2000 characters).</param>
    /// <returns>The persisted <see cref="MessageDto"/>.</returns>
    public async Task<MessageDto> SendMessage(string receiverId, string content)
    {
        if (string.IsNullOrWhiteSpace(receiverId))
            throw new HubException("ReceiverId is required.");
        if (string.IsNullOrWhiteSpace(content))
            throw new HubException("Message content cannot be empty.");
        if (content.Length > 2000)
            throw new HubException("Message cannot exceed 2000 characters.");

        var senderId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        if (senderId == receiverId)
            throw new HubException("Cannot send a message to yourself.");

        var message = await this._messageService.SendMessageAsync(senderId, receiverId, content);
        await Clients.Group($"user_{receiverId}").ReceiveMessage(message);

        // AI bot: show typing indicator, then push the generated reply
        if (receiverId == AiBotConstants.UserId)
        {
            try
            {
                await Clients.Group($"user_{senderId}").UserTyping(AiBotConstants.UserId);
                var aiReply = await this._messageService.SendAiResponseAsync(senderId, content);
                if (aiReply != null)
                    await Clients.Group($"user_{senderId}").ReceiveMessage(aiReply);
            }
            catch { /* AI response failure must never break the user's send */ }
        }

        return message;
    }

    /// <summary>Marks all messages from <paramref name="senderId"/> to the current user as read and notifies the sender.</summary>
    /// <param name="senderId">The identifier of the user whose messages should be marked read.</param>
    public async Task MarkAsRead(string senderId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await this._messageService.MarkAsReadAsync(userId, senderId);
        await Clients.Group($"user_{senderId}").MessageRead(userId);
    }

    /// <summary>Broadcasts a typing indicator to the specified receiver, identifying the current user as the typist.</summary>
    /// <param name="receiverId">The identifier of the user who should see the typing indicator.</param>
    public async Task SendTyping(string receiverId)
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await Clients.Group($"user_{receiverId}").UserTyping(userId);
    }

    /// <summary>Registers the connection in the presence tracker, joins the user's group, and broadcasts an online notification to other clients.</summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        this._presenceTracker.AddConnection(userId, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await Clients.Others.UserOnline(userId);
        await base.OnConnectedAsync();
    }

    /// <summary>Removes the connection from the presence tracker and, if it was the user's last connection, broadcasts an offline notification.</summary>
    /// <param name="exception">The exception that caused the disconnect, if any.</param>
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
/// Strongly-typed SignalR client contract for chat and purchase-request events.
/// </summary>
public interface IChatClient
{
    /// <summary>Invoked when a new chat message is received.</summary>
    Task ReceiveMessage(MessageDto message);
    /// <summary>Invoked to notify that a message has been read by the specified user.</summary>
    Task MessageRead(string readByUserId);
    /// <summary>Invoked to show a typing indicator for the specified user.</summary>
    Task UserTyping(string userId);
    /// <summary>Invoked when the specified user comes online.</summary>
    Task UserOnline(string userId);
    /// <summary>Invoked when the specified user goes offline.</summary>
    Task UserOffline(string userId);

    // Purchase-request events
    /// <summary>Invoked when a new purchase request is received by the seller.</summary>
    Task ReceivePurchaseRequest(PurchaseRequestDto request);
    /// <summary>Invoked when an existing purchase request's status has been updated.</summary>
    Task PurchaseRequestUpdated(PurchaseRequestDto request);
    /// <summary>Invoked when the buyer has chosen a payment method for a purchase request.</summary>
    Task PaymentMethodChosen(object notification);

    // Shipment events
    /// <summary>Invoked when a new shipment has been created for a purchase.</summary>
    Task ShipmentCreated(ShipmentDto shipment);
    /// <summary>Invoked when the status of an existing shipment has changed.</summary>
    Task ShipmentStatusChanged(ShipmentDto shipment);
}
