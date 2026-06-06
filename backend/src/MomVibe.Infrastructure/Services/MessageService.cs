namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly IOutboxWriter _outbox;
    private readonly N8nSettings _n8nSettings;
    private readonly UserPresenceTracker _presenceTracker;
    private readonly IAiService _aiService;
    private readonly IKnowledgeService _knowledge;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IApplicationDbContext context,
        IMapper mapper,
        IOutboxWriter outbox,
        IOptions<N8nSettings> n8nSettings,
        UserPresenceTracker presenceTracker,
        IAiService aiService,
        IKnowledgeService knowledge,
        ILogger<MessageService> logger)
    {
        this._context = context;
        this._mapper = mapper;
        this._outbox = outbox;
        this._n8nSettings = n8nSettings.Value;
        this._presenceTracker = presenceTracker;
        this._aiService = aiService;
        this._knowledge = knowledge;
        this._logger = logger;
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

        // Query 2: Load the last message for each conversation (with navigation properties).
        // We fetch the single most-recent message per peer using stable tie-breaking on Id
        // (descending), which eliminates the timestamp-equality bug that caused wrong messages
        // to be returned when two messages in the same conversation share an identical CreatedAt.
        var latestMessages = await this._context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId)
                     && peerIds.Contains(m.SenderId == userId ? m.ReceiverId : m.SenderId))
            .ToListAsync();

        // Group in-memory and take the newest per peer using stable ordering (time DESC, Id DESC).
        var lastMessageByPeer = latestMessages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(m => m.CreatedAt)
                       .ThenByDescending(m => m.Id)
                       .First());

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
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

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

        // If receiver is offline (and not the AI bot), queue an offline-notification webhook
        // through the transactional outbox. The processor will deliver it with retry + backoff.
        try
        {
            if (receiverId != AiBotConstants.UserId && !this._presenceTracker.IsOnline(receiverId))
            {
                var body = new
                {
                    Event = "chat.message_offline",
                    Timestamp = DateTime.UtcNow,
                    SenderId = senderId,
                    SenderName = saved.Sender?.DisplayName,
                    ReceiverId = receiverId,
                    ReceiverEmail = MaskEmail(saved.Receiver?.Email),
                    ContentPreview = content.Length > 100 ? content[..100] + "..." : content
                };
                this._outbox.Enqueue(OutboxMessageTypes.N8nWebhook, new N8nWebhookOutboxPayload(
                    this._n8nSettings.NewChatMessage,
                    System.Text.Json.JsonSerializer.Serialize(body, OutboxJson)));
                await this._context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Failed to enqueue n8n chat.message_offline for message {MessageId}", saved.Id);
        }

        return this._mapper.Map<MessageDto>(saved);
    }

    private static readonly System.Text.Json.JsonSerializerOptions OutboxJson = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    public async Task MarkAsReadAsync(string userId, string senderId)
    {
        await this._context.Messages
            .Where(m => m.SenderId == senderId && m.ReceiverId == userId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
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

        const string baseSystemPrompt = """
            You are MamVibe Assistant, a helpful AI for the MamVibe marketplace — a Bulgarian platform
            for buying and selling second-hand baby and children's items.

            You help parents with:
            - Pricing advice for listings (typical prices in EUR)
            - Listing tips: what photos to take, how to write a good description
            - Safety tips for meeting buyers or sellers in person
            - How the platform works (posting items, searching, contacting sellers)
            - Care and hygiene tips for second-hand baby items

            Keep responses concise (2-4 sentences). Be warm and friendly — your audience is parents.
            Respond in the same language the user writes in (Bulgarian or English).
            If asked something outside these topics, politely redirect to MamVibe-related questions.
            Never guarantee the safety of any item or fabricate specific product facts.
            Do NOT use markdown formatting (no asterisks, no bold, no bullet dashes) — plain text only.
            """;

        var articles = await this._knowledge.SearchAsync(userMessage, "en");
        var contextBlock = articles.Count > 0
            ? "<context>\n" + string.Join("\n---\n", articles.Select(a => $"{a.Title}\n{a.Content}")) + "\n</context>\n\n"
            : string.Empty;
        var systemPrompt = contextBlock + baseSystemPrompt;

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
