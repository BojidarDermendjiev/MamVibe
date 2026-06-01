namespace MomVibe.Infrastructure.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Application.Events;
using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// On <see cref="PaymentCompletedEvent"/>, fires the n8n <c>payment.completed</c> webhook
/// so external automations (Slack alerts, accounting export, etc.) can pick up the event.
/// Test-mode payments still fire the webhook so n8n flows can be exercised end-to-end
/// in development.
/// </summary>
public sealed class PaymentN8nWebhookHandler : INotificationHandler<PaymentCompletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<PaymentN8nWebhookHandler> _logger;

    public PaymentN8nWebhookHandler(
        IApplicationDbContext context,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings,
        ILogger<PaymentN8nWebhookHandler> logger)
    {
        this._context = context;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
        this._logger = logger;
    }

    public async Task Handle(PaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await this._context.Payments
                .AsNoTracking()
                .Include(p => p.Item).ThenInclude(i => i!.User)
                .Include(p => p.Buyer)
                .FirstOrDefaultAsync(p => p.Id == notification.PaymentId, cancellationToken);
            if (payment is null) return;

            this._webhook.Send(this._n8nSettings.PaymentCompleted, new
            {
                Event = "payment.completed",
                Timestamp = DateTime.UtcNow,
                PaymentId = payment.Id,
                ItemId = payment.ItemId,
                ItemTitle = payment.Item?.Title,
                BuyerEmail = MaskEmail(payment.Buyer?.Email),
                BuyerName = payment.Buyer?.DisplayName,
                SellerEmail = MaskEmail(payment.Item?.User?.Email),
                SellerName = payment.Item?.User?.DisplayName,
                payment.Amount,
                TestMode = notification.IsTestMode
            });
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "n8n payment.completed webhook failed for payment {PaymentId}", notification.PaymentId);
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
