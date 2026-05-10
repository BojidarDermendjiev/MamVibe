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
    /// <summary>Creates a Stripe Checkout session for the specified item and returns the session URL.</summary>
    Task<string> CreateCheckoutSessionAsync(Guid itemId, string buyerId, string successUrl, string cancelUrl, PaymentDeliveryRequest? delivery = null);

    /// <summary>Processes an incoming Stripe webhook event (e.g. checkout.session.completed) and persists the result.</summary>
    Task HandleWebhookAsync(string json, string stripeSignature);

    /// <summary>Creates an on-spot (cash/in-person) payment record for the specified item.</summary>
    Task<PaymentDto> CreateOnSpotPaymentAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null);

    /// <summary>Creates a booking (free reservation) payment record for a donated item.</summary>
    Task<PaymentDto> CreateBookingAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null);

    /// <summary>Returns all payment records in which the specified user is either the buyer or the seller.</summary>
    Task<List<PaymentDto>> GetPaymentsByUserAsync(string userId);

    /// <summary>Returns all payment records on the platform (admin use).</summary>
    Task<List<PaymentDto>> GetAllPaymentsAsync();

    /// <summary>Creates a Stripe PaymentIntent for the specified item and returns the client secret.</summary>
    Task<string> CreatePaymentIntentAsync(Guid itemId, string buyerId);

    /// <summary>Creates a Stripe Checkout session for purchasing multiple items in a single transaction.</summary>
    Task<string> CreateBulkCheckoutSessionAsync(List<Guid> itemIds, string buyerId, string successUrl, string cancelUrl);

    /// <summary>Creates booking payment records for multiple donated items in a single operation.</summary>
    Task<List<PaymentDto>> CreateBulkBookingAsync(List<Guid> itemIds, string buyerId);

    /// <summary>Creates on-spot payment records for multiple items in a single operation.</summary>
    Task<List<PaymentDto>> CreateBulkOnSpotPaymentAsync(List<Guid> itemIds, string buyerId);

    /// <summary>Creates a Stripe Checkout session for a one-time monetary donation and returns the session URL.</summary>
    Task<string> CreateDonationCheckoutAsync(decimal amount, string successUrl, string cancelUrl);

    /// <summary>Creates a Stripe PaymentIntent for a one-time monetary donation and returns the client secret.</summary>
    Task<string> CreateDonationIntentAsync(decimal amount);
}
