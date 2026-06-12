namespace MomVibe.Application.Interfaces;

using DTOs.Payments;

/// <summary>
/// Handles all Stripe-specific payment operations: Checkout sessions, PaymentIntents,
/// bulk/donation sessions, and webhook event processing.
/// </summary>
public interface IStripePaymentService
{
    Task<string> CreateCheckoutSessionAsync(Guid itemId, string buyerId, string successUrl, string cancelUrl, PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null);

    Task HandleWebhookAsync(string json, string stripeSignature);

    Task<string> CreatePaymentIntentAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null);

    Task<string> CreateBulkCheckoutSessionAsync(List<Guid> itemIds, string buyerId, string successUrl, string cancelUrl, string? idempotencyKey = null);

    Task<string> CreateDonationCheckoutAsync(decimal amount, string successUrl, string cancelUrl, string? idempotencyKey = null);

    Task<string> CreateDonationIntentAsync(decimal amount, string? idempotencyKey = null);
}
