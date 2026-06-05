namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Enums;
using Common;
using Constants;

/// <summary>
/// Represents a courier shipment linked to a payment transaction,
/// capturing courier, delivery details, recipient info, financials, and tracking state.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Links to <see cref="Payment"/> (not Item/User directly) because Payment already captures buyer, seller, item, and amount.
/// - Nullable address/office fields with validator enforcement keep the model simple vs. polymorphic hierarchy.
/// - Indexes and column comments are defined in the Infrastructure configuration class.
/// </remarks>
public class Shipment : BaseEntity
{
    /// <summary>
    /// Foreign key referencing the associated payment.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Courier provider used for this shipment.
    /// </summary>
    public CourierProvider CourierProvider { get; set; }

    /// <summary>
    /// Delivery type for this shipment.
    /// </summary>
    public DeliveryType DeliveryType { get; set; }

    /// <summary>
    /// Current shipment lifecycle status.
    /// </summary>
    public ShipmentStatus Status { get; set; } = ShipmentConstants.Defaults.Status;

    /// <summary>
    /// Courier tracking number for package lookup.
    /// </summary>
    [MaxLength(ShipmentConstants.Lengths.TrackingNumberMax)]
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Courier waybill identifier for API operations.
    /// </summary>
    [MaxLength(ShipmentConstants.Lengths.WaybillIdMax)]
    public string? WaybillId { get; set; }

    /// <summary>
    /// Full name of the shipment recipient.
    /// </summary>
    [Required]
    [MaxLength(ShipmentConstants.Lengths.RecipientNameMax)]
    public required string RecipientName { get; set; }

    /// <summary>
    /// Phone number of the shipment recipient.
    /// </summary>
    [Required]
    [MaxLength(ShipmentConstants.Lengths.RecipientPhoneMax)]
    public required string RecipientPhone { get; set; }

    /// <summary>
    /// Street address for address-based delivery.
    /// </summary>
    [MaxLength(ShipmentConstants.Lengths.DeliveryAddressMax)]
    public string? DeliveryAddress { get; set; }

    /// <summary>
    /// City name for delivery destination.
    /// </summary>
    [MaxLength(ShipmentConstants.Lengths.CityMax)]
    public string? City { get; set; }

    /// <summary>
    /// Courier office or locker identifier.
    /// </summary>
    [MaxLength(ShipmentConstants.Lengths.OfficeIdMax)]
    public string? OfficeId { get; set; }

    /// <summary>
    /// Courier office or locker display name.
    /// </summary>
    [MaxLength(ShipmentConstants.Lengths.OfficeNameMax)]
    public string? OfficeName { get; set; }

    /// <summary>
    /// Shipping price charged for this shipment.
    /// </summary>
    public decimal ShippingPrice { get; set; }

    /// <summary>
    /// Whether cash on delivery is enabled.
    /// </summary>
    public bool IsCod { get; set; }

    /// <summary>
    /// Cash on delivery amount to collect from recipient.
    /// </summary>
    public decimal CodAmount { get; set; }

    /// <summary>
    /// Whether the shipment has additional insurance.
    /// </summary>
    public bool IsInsured { get; set; }

    /// <summary>
    /// Declared value for shipment insurance.
    /// </summary>
    public decimal InsuredAmount { get; set; }

    /// <summary>
    /// Package weight in kilograms.
    /// </summary>
    public decimal Weight { get; set; } = ShipmentConstants.Defaults.Weight;

    /// <summary>
    /// URL or path to the generated shipping label PDF.
    /// </summary>
    [MaxLength(ShipmentConstants.Lengths.LabelUrlMax)]
    public string? LabelUrl { get; set; }

    /// <summary>
    /// Navigation to the associated payment.
    /// </summary>
    public Payment Payment { get; set; } = null!;
}
