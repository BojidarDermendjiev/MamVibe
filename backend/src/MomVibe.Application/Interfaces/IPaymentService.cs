namespace MomVibe.Application.Interfaces;

using DTOs.Payments;

/// <summary>
/// Payment service contract:
/// - Creates a Stripe Checkout session for a sellable item and returns the session URL.
/// - Handles Stripe webhook payloads (e.g., checkout.session.completed) to persist payment results.
/// - Creates an on-spot (offline) payment record.
/// - Retrieves payments associated with a given user (as buyer or seller).
/// </summary>
public interface IPaymentService
{
    Task<string> CreateCheckoutSessionAsync(Guid itemId, string buyerId, string successUrl, string cancelUrl);
    Task HandleWebhookAsync(string json, string stripeSignature);
    Task<PaymentDto> CreateOnSpotPaymentAsync(Guid itemId, string buyerId);
    Task<PaymentDto> CreateBookingAsync(Guid itemId, string buyerId);
    Task<List<PaymentDto>> GetPaymentsByUserAsync(string userId);
    Task<List<PaymentDto>> GetAllPaymentsAsync();
    Task<string> CreatePaymentIntentAsync(Guid itemId, string buyerId);
}
