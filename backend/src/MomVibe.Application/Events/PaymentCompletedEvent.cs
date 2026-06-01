namespace MomVibe.Application.Events;

/// <summary>
/// Raised after a Payment row reaches <c>PaymentStatus.Completed</c> and has been persisted.
/// Subscribers handle the downstream side-effects that used to live inline in
/// <c>PaymentService.HandleWebhookAsync</c> and <c>PaymentService.Create*Async</c>:
/// digital receipt creation, e-bill issuance, n8n payment-completed webhook,
/// and the purchase-request status update.
/// </summary>
/// <param name="PaymentId">The completed payment's identifier.</param>
/// <param name="IsTestMode">
/// True when the payment was simulated because Stripe was not configured;
/// handlers may skip side-effects (e-bill emails, take-a-nap receipts) that
/// require real production data.
/// </param>
public sealed record PaymentCompletedEvent(Guid PaymentId, bool IsTestMode = false) : IDomainEvent;
