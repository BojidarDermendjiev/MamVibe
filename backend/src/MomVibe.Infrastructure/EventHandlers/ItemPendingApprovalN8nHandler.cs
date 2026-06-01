namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Application.Events;
using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// On <see cref="ItemCreatedEvent"/>, fires the n8n <c>item.pending_approval</c> webhook
/// when the AI moderation result left the item inactive (NeedsReview / FlaggedForReview).
/// Auto-approved items skip this handler — there's nothing for an admin to triage.
/// </summary>
public sealed class ItemPendingApprovalN8nHandler : INotificationHandler<ItemCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<ItemPendingApprovalN8nHandler> _logger;

    public ItemPendingApprovalN8nHandler(
        IApplicationDbContext context,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings,
        ILogger<ItemPendingApprovalN8nHandler> logger)
    {
        this._context = context;
        this._webhook = webhook;
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

            this._webhook.Send(this._n8nSettings.ItemPendingApproval, new
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
            });
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "n8n item.pending_approval webhook failed for item {ItemId}", notification.ItemId);
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
