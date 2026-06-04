namespace MomVibe.Application.Interfaces;

using DTOs.Common;
using DTOs.Payments;

/// <summary>
/// Handles non-Stripe payment methods (on-spot, cash-on-delivery, booking) and payment queries.
/// Stripe-specific flows (checkout sessions, webhooks, intents) live in <see cref="IStripePaymentService"/>.
///
/// Idempotency: create-* methods accept an optional idempotencyKey and dedupe against an existing
/// Payment row with the same key created in the last 24 hours; the unique DB index is the safety
/// net for concurrent races.
/// </summary>
public interface IPaymentService
{
    Task<PaymentDto> CreateOnSpotPaymentAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null);

    Task<PaymentDto> CreateBookingAsync(Guid itemId, string buyerId, PaymentDeliveryRequest? delivery = null, string? idempotencyKey = null);

    Task<PaymentDto> CreateCashOnDeliveryAsync(Guid itemId, string buyerId, PaymentDeliveryRequest delivery, string? idempotencyKey = null);

    Task<List<PaymentDto>> CreateBulkBookingAsync(List<Guid> itemIds, string buyerId, string? idempotencyKey = null);

    Task<List<PaymentDto>> CreateBulkOnSpotPaymentAsync(List<Guid> itemIds, string buyerId, string? idempotencyKey = null);

    Task<PagedResult<PaymentDto>> GetPaymentsByUserAsync(string userId, int page = 1, int pageSize = 20);

    Task<PagedResult<PaymentDto>> GetAllPaymentsAsync(int page = 1, int pageSize = 50);
}
