namespace MomVibe.Application.Interfaces;

using DTOs.Bundles;
using DTOs.Payments;

/// <summary>
/// Business logic for seller bundles: creation, retrieval, deletion, and payment flows.
/// </summary>
public interface IBundleService
{
    /// <summary>
    /// Creates a new bundle for the given seller using the supplied item IDs.
    /// Validates that all items belong to the seller, are active, and are not already in another active bundle.
    /// </summary>
    /// <param name="sellerId">Identity of the authenticated seller.</param>
    /// <param name="dto">Bundle creation payload.</param>
    /// <returns>The fully populated <see cref="BundleDto"/> for the newly created bundle.</returns>
    Task<BundleDto> CreateAsync(string sellerId, CreateBundleDto dto);

    /// <summary>
    /// Retrieves a single bundle by its identifier, including item details and seller info.
    /// </summary>
    /// <param name="id">The bundle's unique identifier.</param>
    /// <returns>The <see cref="BundleDto"/>, or throws <see cref="KeyNotFoundException"/> if not found.</returns>
    Task<BundleDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Returns all bundles created by the given seller.
    /// </summary>
    /// <param name="sellerId">Identity of the seller.</param>
    Task<List<BundleDto>> GetMyAsync(string sellerId);

    /// <summary>
    /// Deletes a bundle created by the seller. Throws if the bundle is already sold.
    /// </summary>
    /// <param name="id">The bundle's unique identifier.</param>
    /// <param name="sellerId">Identity of the authenticated seller (ownership check).</param>
    Task DeleteAsync(Guid id, string sellerId);

    /// <summary>
    /// Creates (or simulates) a Stripe Checkout session for purchasing the bundle.
    /// In test mode (no valid Stripe key) simulates an immediate completed payment.
    /// </summary>
    /// <param name="bundleId">The bundle to purchase.</param>
    /// <param name="buyerId">Identity of the authenticated buyer.</param>
    /// <param name="successUrl">URL to redirect to after successful payment.</param>
    /// <param name="cancelUrl">URL to redirect to if the buyer cancels.</param>
    /// <param name="delivery">Optional delivery information for automatic shipment creation.</param>
    /// <returns>The Stripe Checkout URL, or the success URL with <c>?session_id=test_simulated</c> in test mode.</returns>
    Task<string> CreateCheckoutSessionAsync(Guid bundleId, string buyerId, string successUrl, string cancelUrl, PaymentDeliveryRequest? delivery = null);

    /// <summary>
    /// Creates an on-spot (cash/offline) payment record for a bundle and marks it as pending.
    /// </summary>
    /// <param name="bundleId">The bundle to pay for.</param>
    /// <param name="buyerId">Identity of the authenticated buyer.</param>
    /// <param name="delivery">Optional delivery information for automatic shipment creation.</param>
    Task<PaymentDto> CreateOnSpotPaymentAsync(Guid bundleId, string buyerId, PaymentDeliveryRequest? delivery = null);

    /// <summary>
    /// Creates a cash-on-delivery payment record for a bundle, calculates the shipping fee,
    /// creates the courier shipment, and marks the bundle as sold.
    /// </summary>
    /// <param name="bundleId">The bundle to pay for via COD.</param>
    /// <param name="buyerId">Identity of the authenticated buyer.</param>
    /// <param name="delivery">Delivery information required for COD shipment creation.</param>
    Task<PaymentDto> CreateCashOnDeliveryAsync(Guid bundleId, string buyerId, PaymentDeliveryRequest delivery);
}
