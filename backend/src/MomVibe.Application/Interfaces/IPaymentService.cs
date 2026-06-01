namespace MomVibe.Application.Interfaces;

using DTOs.Common;
using DTOs.Payments;

/// <summary>
/// Payment service contract:
/// - Creates a Stripe Checkout session for a sellable item and returns the session URL.
/// - Handles Stripe webhook payloads (e.g., checkout.session.completed) to persist payment results.
/// - Creates an on-spot (offline) payment record.
/// - Retrieves payments associated with a given user (as buyer or seller).
///
/// Idempotency: the create-* methods accept an optional <c>idempotencyKey</c>. Methods that persist
/// a <c>Payment</c> row at request time (OnSpot, COD, Booking, test-mode Stripe, bulk variants)
/// dedupe against an existing row with the same key created in the last 24 hours; the unique index
/// on <c>Payment.IdempotencyKey</c> is the safety net for races. Methods that talk to Stripe
/// forward the key via <c>RequestOptions.IdempotencyKey</c> so Stripe dedupes server-side.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a Stripe Checkout session for the specified item and returns the session URL.
    /// When <paramref name="idempotencyKey"/> is supplied it is forwarded to Stripe and (in test mode) used to dedupe duplicate Payment rows.
    /// </summary>
    Task<string> CreateCheckoutSessionAsync(Guid itemId, string buyerId, string successUrl, string cancelUrl, PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null);

    /// <summary>Processes an incoming Stripe webhook event (e.g. checkout.session.completed) and persists the result.</summary>
    Task HandleWebhookAsync(string json, string stripeSignature);

    /// <summary>Creates an on-spot (cash/in-person) payment record. Deduplicates against an existing Payment with the same <paramref name="idempotencyKey"/> created in the last 24 hours.</summary>
    Task<PaymentDto> CreateOnSpotPaymentAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null);

    /// <summary>Creates a booking (free reservation) payment record for a donated item. Deduplicates by <paramref name="idempotencyKey"/>.</summary>
    Task<PaymentDto> CreateBookingAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null);

    /// <summary>Creates a cash-on-delivery payment record. Amount is collected by the courier at delivery. Deduplicates by <paramref name="idempotencyKey"/>.</summary>
    Task<PaymentDto> CreateCashOnDeliveryAsync(Guid itemId, string buyerId, PaymentDeliveryRequest delivery, string? idempotencyKey = null);

    /// <summary>Returns a paginated page of payment records in which the specified user is either the buyer or the seller.</summary>
    Task<PagedResult<PaymentDto>> GetPaymentsByUserAsync(string userId, int page = 1, int pageSize = 20);

    /// <summary>Returns a paginated page of payment records for admin oversight.</summary>
    Task<PagedResult<PaymentDto>> GetAllPaymentsAsync(int page = 1, int pageSize = 50);

    /// <summary>Creates a Stripe PaymentIntent for the specified item and returns the client secret. Forwards <paramref name="idempotencyKey"/> to Stripe.</summary>
    Task<string> CreatePaymentIntentAsync(Guid itemId, string buyerId, string? idempotencyKey = null);

    /// <summary>Creates a Stripe Checkout session for purchasing multiple items in a single transaction. Forwards <paramref name="idempotencyKey"/> to Stripe and dedupes in test mode.</summary>
    Task<string> CreateBulkCheckoutSessionAsync(List<Guid> itemIds, string buyerId, string successUrl, string cancelUrl, string? idempotencyKey = null);

    /// <summary>Creates booking payment records for multiple donated items in a single operation. Deduplicates by <paramref name="idempotencyKey"/>.</summary>
    Task<List<PaymentDto>> CreateBulkBookingAsync(List<Guid> itemIds, string buyerId, string? idempotencyKey = null);

    /// <summary>Creates on-spot payment records for multiple items in a single operation. Deduplicates by <paramref name="idempotencyKey"/>.</summary>
    Task<List<PaymentDto>> CreateBulkOnSpotPaymentAsync(List<Guid> itemIds, string buyerId, string? idempotencyKey = null);

    /// <summary>Creates a Stripe Checkout session for a one-time monetary donation and returns the session URL. Forwards <paramref name="idempotencyKey"/> to Stripe.</summary>
    Task<string> CreateDonationCheckoutAsync(decimal amount, string successUrl, string cancelUrl, string? idempotencyKey = null);

    /// <summary>Creates a Stripe PaymentIntent for a one-time monetary donation and returns the client secret. Forwards <paramref name="idempotencyKey"/> to Stripe.</summary>
    Task<string> CreateDonationIntentAsync(decimal amount, string? idempotencyKey = null);
}
