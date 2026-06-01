namespace MomVibe.Infrastructure.EventHandlers;

using System.Text.Json;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Application.Events;
using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// On <see cref="ItemCreatedEvent"/>, queues the n8n <c>item.pending_approval</c> webhook through
/// the transactional outbox when the AI moderation result left the item inactive. Auto-approved
/// items skip the enqueue — there's nothing for an admin to triage.
/// </summary>
public sealed class ItemPendingApprovalN8nHandler : INotificationHandler<ItemCreatedEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IApplicationDbContext _context;
    private readonly IOutboxWriter _outbox;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<ItemPendingApprovalN8nHandler> _logger;

    public ItemPendingApprovalN8nHandler(
        IApplicationDbContext context,
        IOutboxWriter outbox,
        IOptions<N8nSettings> n8nSettings,
        ILogger<ItemPendingApprovalN8nHandler> logger)
    {
        this._context = context;
        this._outbox = outbox;
        this._n8nSettings = n8nSettings.Value;
        this._logger = logger;
    }

    public async Task Handle(ItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var item = await this._context.Items
                .AsNoTracking()
                .Include(i => i.User)
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == notification.ItemId, cancellationToken);
            if (item is null || item.IsActive) return;

            var body = new
            {
                Event = "item.pending_approval",
                Timestamp = DateTime.UtcNow,
                ItemId = item.Id,
                item.Title,
                item.Description,
                Category = item.Category?.Name,
                ListingType = item.ListingType.ToString(),
                item.Price,
                SellerName = item.User?.DisplayName,
                SellerEmail = MaskEmail(item.User?.Email),
                AiRecommendation = item.AiModerationStatus.ToString(),
                AiReason = item.AiModerationNotes
            };

            this._outbox.Enqueue(OutboxMessageTypes.N8nWebhook, new N8nWebhookOutboxPayload(
                this._n8nSettings.ItemPendingApproval,
                JsonSerializer.Serialize(body, JsonOptions)));
            await this._context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Failed to enqueue n8n item.pending_approval for item {ItemId}", notification.ItemId);
        }
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
}
