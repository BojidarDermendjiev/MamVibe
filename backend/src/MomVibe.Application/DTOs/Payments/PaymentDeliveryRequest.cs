namespace MomVibe.Application.DTOs.Payments;

using Domain.Enums;

/// <summary>
/// Optional delivery information attached to a payment request.
/// When provided, the payment service will automatically create a courier shipment.
/// </summary>
public class PaymentDeliveryRequest
{
    /// <summary>Gets or sets the courier provider to use for the auto-created shipment.</summary>
    public CourierProvider CourierProvider { get; set; }
    /// <summary>Gets or sets the delivery method (address, office, or locker).</summary>
    public DeliveryType DeliveryType { get; set; }
    /// <summary>Gets or sets the full name of the shipment recipient.</summary>
    public string RecipientName { get; set; } = string.Empty;
    /// <summary>Gets or sets the phone number of the shipment recipient.</summary>
    public string RecipientPhone { get; set; } = string.Empty;
    /// <summary>Gets or sets the destination city; used for both address and office deliveries.</summary>
    public string? City { get; set; }
    /// <summary>Gets or sets the street address for door-to-door delivery.</summary>
    public string? Address { get; set; }
    /// <summary>Gets or sets the courier-specific office or locker identifier for office/locker delivery.</summary>
    public string? OfficeId { get; set; }
    /// <summary>Gets or sets the human-readable name of the target office or locker.</summary>
    public string? OfficeName { get; set; }
    /// <summary>Gets or sets the package weight in kilograms (default: 1 kg).</summary>
    public decimal Weight { get; set; } = 1m;
}
