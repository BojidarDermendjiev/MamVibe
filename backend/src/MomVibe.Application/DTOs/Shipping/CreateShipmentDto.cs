namespace MomVibe.Application.DTOs.Shipping;

using Domain.Enums;

/// <summary>
/// DTO for creating a new shipment:
/// - PaymentId: links to the payment transaction.
/// - CourierProvider and DeliveryType: courier and delivery method selection.
/// - Recipient: name and phone of the recipient.
/// - Delivery: address/city for Address delivery, OfficeId/OfficeName for Office/Locker delivery.
/// - Financial: weight, COD toggle and amount, insurance toggle and amount.
/// </summary>
public class CreateShipmentDto
{
    /// <summary>Gets or sets the identifier of the payment this shipment is associated with.</summary>
    public Guid PaymentId { get; set; }
    /// <summary>Gets or sets the courier provider to create the shipment with (Econt, Speedy, BoxNow, etc.).</summary>
    public CourierProvider CourierProvider { get; set; }
    /// <summary>Gets or sets the delivery method (address, office, or locker).</summary>
    public DeliveryType DeliveryType { get; set; }
    /// <summary>Gets or sets the full name of the shipment recipient.</summary>
    public required string RecipientName { get; set; }
    /// <summary>Gets or sets the phone number of the shipment recipient.</summary>
    public required string RecipientPhone { get; set; }
    /// <summary>Gets or sets the street address for door-to-door delivery; required when DeliveryType is Address.</summary>
    public string? DeliveryAddress { get; set; }
    /// <summary>Gets or sets the destination city; used for both address and office deliveries.</summary>
    public string? City { get; set; }
    /// <summary>Gets or sets the courier-specific office or locker identifier; required for Office/Locker delivery.</summary>
    public string? OfficeId { get; set; }
    /// <summary>Gets or sets the human-readable name of the target office or locker, for label printing.</summary>
    public string? OfficeName { get; set; }
    /// <summary>Gets or sets the package weight in kilograms.</summary>
    public decimal Weight { get; set; }
    /// <summary>Gets or sets a value indicating whether cash-on-delivery is enabled for this shipment.</summary>
    public bool IsCod { get; set; }
    /// <summary>Gets or sets the amount the courier should collect on delivery in BGN.</summary>
    public decimal CodAmount { get; set; }
    /// <summary>Gets or sets a value indicating whether shipment insurance is enabled.</summary>
    public bool IsInsured { get; set; }
    /// <summary>Gets or sets the declared value in BGN for insurance premium calculation.</summary>
    public decimal InsuredAmount { get; set; }
}
