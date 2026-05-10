namespace MomVibe.Application.DTOs.Shipping;

using Domain.Enums;

/// <summary>
/// DTO representing a shipment record:
/// - Identity &amp; linkage: Id, PaymentId, optional ItemTitle from Payment.Item.
/// - Courier: CourierProvider, DeliveryType, Status.
/// - Tracking: TrackingNumber, WaybillId.
/// - Recipient: RecipientName, RecipientPhone.
/// - Delivery: DeliveryAddress, City, OfficeId, OfficeName.
/// - Financial: ShippingPrice, IsCod, CodAmount, IsInsured, InsuredAmount.
/// - Package: Weight.
/// - Label: LabelUrl.
/// - Timestamps: CreatedAt.
/// </summary>
public class ShipmentDto
{
    /// <summary>Gets or sets the unique identifier of this shipment record.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the identifier of the payment linked to this shipment.</summary>
    public Guid PaymentId { get; set; }
    /// <summary>Gets or sets the title of the purchased item, for display purposes.</summary>
    public string? ItemTitle { get; set; }
    /// <summary>Gets or sets the courier provider handling this shipment.</summary>
    public CourierProvider CourierProvider { get; set; }
    /// <summary>Gets or sets the delivery method (address, office, or locker).</summary>
    public DeliveryType DeliveryType { get; set; }
    /// <summary>Gets or sets the current status of the shipment in its lifecycle.</summary>
    public ShipmentStatus Status { get; set; }
    /// <summary>Gets or sets the courier-assigned tracking number for this shipment.</summary>
    public string? TrackingNumber { get; set; }
    /// <summary>Gets or sets the courier-internal waybill identifier used for label generation and cancellation.</summary>
    public string? WaybillId { get; set; }
    /// <summary>Gets or sets the full name of the shipment recipient.</summary>
    public required string RecipientName { get; set; }
    /// <summary>Gets or sets the phone number of the shipment recipient.</summary>
    public required string RecipientPhone { get; set; }
    /// <summary>Gets or sets the street address for door-to-door delivery; null for office/locker deliveries.</summary>
    public string? DeliveryAddress { get; set; }
    /// <summary>Gets or sets the destination city.</summary>
    public string? City { get; set; }
    /// <summary>Gets or sets the target office or locker identifier; null for address deliveries.</summary>
    public string? OfficeId { get; set; }
    /// <summary>Gets or sets the human-readable name of the target office or locker.</summary>
    public string? OfficeName { get; set; }
    /// <summary>Gets or sets the total shipping cost for this shipment in BGN.</summary>
    public decimal ShippingPrice { get; set; }
    /// <summary>Gets or sets a value indicating whether cash-on-delivery is enabled.</summary>
    public bool IsCod { get; set; }
    /// <summary>Gets or sets the cash-on-delivery amount the courier collects in BGN.</summary>
    public decimal CodAmount { get; set; }
    /// <summary>Gets or sets a value indicating whether shipment insurance is active.</summary>
    public bool IsInsured { get; set; }
    /// <summary>Gets or sets the declared insured value in BGN.</summary>
    public decimal InsuredAmount { get; set; }
    /// <summary>Gets or sets the package weight in kilograms.</summary>
    public decimal Weight { get; set; }
    /// <summary>Gets or sets the URL to the printable shipping label PDF.</summary>
    public string? LabelUrl { get; set; }
    /// <summary>Gets or sets the timestamp when this shipment record was created.</summary>
    public DateTime CreatedAt { get; set; }
}
