namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Domain.Entities;
using Application.Interfaces;
using Application.DTOs.Messages;
using Infrastructure.Configuration;

/// <summary>
/// Service for user-to-user messaging: retrieves conversations with last message and unread count,
/// paginates message threads, sends messages, and marks messages as read. Uses EF Core to load
/// participants (sender/receiver) and AutoMapper to map entities to DTOs.
/// </summary>
public class MessageService : IMessageService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;
    private readonly UserPresenceTracker _presenceTracker;

    public MessageService(
        IApplicationDbContext context,
        IMapper mapper,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings,
        UserPresenceTracker presenceTracker)
    {
        this._context = context;
        this._mapper = mapper;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
        this._presenceTracker = presenceTracker;
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return "***";
        var at = email.IndexOf('@');
        if (at <= 0) return "***";
        var local = email[..at];
        var domain = email[at..];
        return (local.Length <= 2 ? "***" : local[..2] + "***") + domain;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(string userId)
    {
        var messages = await this._context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var otherUser = g.First().SenderId == userId ? g.First().Receiver : g.First().Sender;
                var lastMessage = g.First();
                var unreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead);

                return new ConversationDto
                {
                    UserId = otherUser.Id,
                    DisplayName = otherUser.DisplayName,
                    AvatarUrl = otherUser.AvatarUrl,
                    LastMessage = lastMessage.Content,
                    LastMessageTime = lastMessage.CreatedAt,
                    UnreadCount = unreadCount
                };
            })
            .ToList();

        return conversations;
    }

    public async Task<List<MessageDto>> GetMessagesAsync(string userId, string otherUserId, int page = 1, int pageSize = 50)
    {
        var messages = await this._context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return this._mapper.Map<List<MessageDto>>(messages);
    }

    public async Task<MessageDto> SendMessageAsync(string senderId, string receiverId, string content)
    {
        if (senderId == receiverId)
            throw new InvalidOperationException("You cannot send a message to yourself.");

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.");

        content = content.Trim();
        if (content.Length > 2000)
            throw new ArgumentException("Message cannot exceed 2000 characters.");

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content
        };

        this._context.Messages.Add(message);
        await this._context.SaveChangesAsync();

        var saved = await this._context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstAsync(m => m.Id == message.Id);

        // If receiver is offline, fire webhook for offline notification
        try
        {
            if (!this._presenceTracker.IsOnline(receiverId))
            {
                this._webhook.Send(this._n8nSettings.NewChatMessage, new
                {
                    Event = "chat.message_offline",
                    Timestamp = DateTime.UtcNow,
                    SenderId = senderId,
                    SenderName = saved.Sender?.DisplayName,
                    ReceiverId = receiverId,
                    ReceiverEmail = MaskEmail(saved.Receiver?.Email),
                    ContentPreview = content.Length > 100 ? content[..100] + "..." : content
                });
            }
        }
        catch { /* Webhook failure must not break message flow */ }

        return this._mapper.Map<MessageDto>(saved);
    }

    public async Task MarkAsReadAsync(string userId, string senderId)
    {
        var unreadMessages = await this._context.Messages
            .Where(m => m.SenderId == senderId && m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await this._context.SaveChangesAsync();
    }
}
