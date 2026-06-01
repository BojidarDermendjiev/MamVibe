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
/// On <see cref="PaymentCompletedEvent"/>, queues the n8n <c>payment.completed</c> webhook through
/// the transactional outbox. Writing to the outbox (instead of calling <see cref="IN8nWebhookService"/>
/// directly) means the webhook survives an API restart between the moment the Payment row commits
/// and the moment n8n actually receives the call.
/// </summary>
public sealed class PaymentN8nWebhookHandler : INotificationHandler<PaymentCompletedEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IApplicationDbContext _context;
    private readonly IOutboxWriter _outbox;
    private readonly N8nSettings _n8nSettings;
    private readonly ILogger<PaymentN8nWebhookHandler> _logger;

    public PaymentN8nWebhookHandler(
        IApplicationDbContext context,
        IOutboxWriter outbox,
        IOptions<N8nSettings> n8nSettings,
        ILogger<PaymentN8nWebhookHandler> logger)
    {
        this._context = context;
        this._outbox = outbox;
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

            var body = new
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
            };

            this._outbox.Enqueue(OutboxMessageTypes.N8nWebhook, new N8nWebhookOutboxPayload(
                this._n8nSettings.PaymentCompleted,
                JsonSerializer.Serialize(body, JsonOptions)));
            await this._context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Failed to enqueue n8n payment.completed for payment {PaymentId}", notification.PaymentId);
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
