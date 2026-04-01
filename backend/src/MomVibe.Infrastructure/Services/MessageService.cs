namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Domain.Entities;
using Domain.Constants;
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
    private readonly IAiService _aiService;

    public MessageService(
        IApplicationDbContext context,
        IMapper mapper,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings,
        UserPresenceTracker presenceTracker,
        IAiService aiService)
    {
        this._context = context;
        this._mapper = mapper;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
        this._presenceTracker = presenceTracker;
        this._aiService = aiService;
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
        // Query 1: SQL-level GROUP BY — gets per-peer aggregates without loading all messages.
        // Max 50 conversations returned to prevent unbounded result sets.
        var summaries = await this._context.Messages
            .AsNoTracking()
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => new
            {
                PeerId = g.Key,
                LastMessageTime = g.Max(m => m.CreatedAt),
                UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead),
            })
            .OrderByDescending(g => g.LastMessageTime)
            .Take(50)
            .ToListAsync();

        if (summaries.Count == 0)
            return new List<ConversationDto>();

        var peerIds = summaries.Select(s => s.PeerId).ToList();
        var maxTimes = summaries.Select(s => s.LastMessageTime).Distinct().ToList();

        // Query 2: Load the last message for each conversation (with navigation properties)
        // using the known max timestamps — bounded to 50 peer conversations.
        var latestMessages = await this._context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId)
                     && maxTimes.Contains(m.CreatedAt))
            .ToListAsync();

        // Match each peer to its most recent message (ordered by time to handle ties)
        var lastMessageByPeer = new Dictionary<string, Message>();
        foreach (var s in summaries)
        {
            var msg = latestMessages
                .Where(m => m.CreatedAt == s.LastMessageTime
                         && (m.SenderId == s.PeerId || m.ReceiverId == s.PeerId))
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();
            if (msg != null)
                lastMessageByPeer[s.PeerId] = msg;
        }

        return summaries
            .Where(s => lastMessageByPeer.ContainsKey(s.PeerId))
            .Select(s =>
            {
                var msg = lastMessageByPeer[s.PeerId];
                var otherUser = msg.SenderId == userId ? msg.Receiver : msg.Sender;
                return new ConversationDto
                {
                    UserId = s.PeerId,
                    DisplayName = otherUser?.DisplayName ?? "Unknown",
                    AvatarUrl = otherUser?.AvatarUrl,
                    LastMessage = msg.Content,
                    LastMessageTime = s.LastMessageTime,
                    UnreadCount = s.UnreadCount,
                };
            })
            .ToList();
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

        // If receiver is offline (and not the AI bot), fire webhook for offline notification
        try
        {
            if (receiverId != AiBotConstants.UserId && !this._presenceTracker.IsOnline(receiverId))
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

    public async Task<MessageDto?> SendAiResponseAsync(string userId, string userMessage)
    {
        // Load the last 10 messages of this conversation for context (includes the just-saved user message)
        var history = await this._context.Messages
            .AsNoTracking()
            .Where(m => (m.SenderId == userId       && m.ReceiverId == AiBotConstants.UserId) ||
                        (m.SenderId == AiBotConstants.UserId && m.ReceiverId == userId))
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new { m.SenderId, m.Content })
            .ToListAsync();

        var conversationHistory = history
            .Select(m => (
                role: m.SenderId == userId ? "user" : "assistant",
                content: m.Content
            ))
            .ToList<(string role, string content)>();

        const string systemPrompt = """
            You are MamVibe Assistant, a helpful AI for the MamVibe marketplace — a Bulgarian platform
            for buying and selling second-hand baby and children's items.

            You help parents with:
            - Pricing advice for listings (typical prices in BGN)
            - Listing tips: what photos to take, how to write a good description
            - Safety tips for meeting buyers or sellers in person
            - How the platform works (posting items, searching, contacting sellers)
            - Care and hygiene tips for second-hand baby items

            Keep responses concise (2-4 sentences). Be warm and friendly — your audience is parents.
            Respond in the same language the user writes in (Bulgarian or English).
            If asked something outside these topics, politely redirect to MamVibe-related questions.
            Never guarantee the safety of any item or fabricate specific product facts.
            """;

        string responseText;
        try
        {
            responseText = await this._aiService.ChatAsync(systemPrompt, conversationHistory);
        }
        catch
        {
            return null; // AI failure must never break the chat
        }

        try
        {
            var botMessage = new Message
            {
                SenderId = AiBotConstants.UserId,
                ReceiverId = userId,
                Content = responseText
            };

            this._context.Messages.Add(botMessage);
            await this._context.SaveChangesAsync();

            var saved = await this._context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstAsync(m => m.Id == botMessage.Id);

            return this._mapper.Map<MessageDto>(saved);
        }
        catch
        {
            return null; // DB save failure (e.g. bot user not yet seeded) must not crash the hub
        }
    }
}
