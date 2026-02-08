namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.Shipment"/> to keep validation,
/// defaults, and database comments consistent across the codebase.
/// </summary>
public static class ShipmentConstants
{
    /// <summary>
    /// Length-related constraints for <see cref="MomVibe.Domain.Entities.Shipment"/> properties.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Maximum length for <c>TrackingNumber</c>.</summary>
        public const int TrackingNumberMax = 100;

        /// <summary>Maximum length for <c>WaybillId</c>.</summary>
        public const int WaybillIdMax = 100;

        /// <summary>Maximum length for <c>RecipientName</c>.</summary>
        public const int RecipientNameMax = 200;

        /// <summary>Maximum length for <c>RecipientPhone</c>.</summary>
        public const int RecipientPhoneMax = 30;

        /// <summary>Maximum length for <c>DeliveryAddress</c>.</summary>
        public const int DeliveryAddressMax = 500;

        /// <summary>Maximum length for <c>City</c>.</summary>
        public const int CityMax = 200;

        /// <summary>Maximum length for <c>OfficeId</c>.</summary>
        public const int OfficeIdMax = 50;

        /// <summary>Maximum length for <c>OfficeName</c>.</summary>
        public const int OfficeNameMax = 300;

        /// <summary>Maximum length for <c>LabelUrl</c>.</summary>
        public const int LabelUrlMax = 1000;
    }

    /// <summary>
    /// Default values for <see cref="MomVibe.Domain.Entities.Shipment"/>.
    /// </summary>
    public static class Defaults
    {
        /// <summary>Default shipment status.</summary>
        public const MomVibe.Domain.Enums.ShipmentStatus Status = MomVibe.Domain.Enums.ShipmentStatus.Pending;

        /// <summary>Default weight in kilograms.</summary>
        public const decimal Weight = 1.0m;
    }

    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    public static class Comments
    {
        /// <summary>Column comment: foreign key referencing the associated payment.</summary>
        public const string PaymentId = "Foreign key referencing the associated payment.";

        /// <summary>Column comment: courier provider used for this shipment.</summary>
        public const string CourierProvider = "Courier provider used for this shipment (Econt, Speedy).";

        /// <summary>Column comment: delivery type for this shipment.</summary>
        public const string DeliveryType = "Delivery type (Office, Address, Locker).";

        /// <summary>Column comment: current shipment status.</summary>
        public const string Status = "Current shipment lifecycle status.";

        /// <summary>Column comment: courier tracking number.</summary>
        public const string TrackingNumber = "Courier tracking number for package lookup.";

        /// <summary>Column comment: courier waybill identifier.</summary>
        public const string WaybillId = "Courier waybill identifier for API operations.";

        /// <summary>Column comment: recipient full name.</summary>
        public const string RecipientName = "Full name of the shipment recipient.";

        /// <summary>Column comment: recipient phone number.</summary>
        public const string RecipientPhone = "Phone number of the shipment recipient.";

        /// <summary>Column comment: delivery street address.</summary>
        public const string DeliveryAddress = "Street address for address-based delivery.";

        /// <summary>Column comment: delivery city name.</summary>
        public const string City = "City name for delivery destination.";

        /// <summary>Column comment: courier office identifier.</summary>
        public const string OfficeId = "Courier office or locker identifier.";

        /// <summary>Column comment: courier office display name.</summary>
        public const string OfficeName = "Courier office or locker display name.";

        /// <summary>Column comment: shipping price charged.</summary>
        public const string ShippingPrice = "Shipping price charged for this shipment.";

        /// <summary>Column comment: whether cash on delivery is enabled.</summary>
        public const string IsCod = "Whether cash on delivery is enabled.";

        /// <summary>Column comment: cash on delivery amount.</summary>
        public const string CodAmount = "Cash on delivery amount to collect from recipient.";

        /// <summary>Column comment: whether shipment is insured.</summary>
        public const string IsInsured = "Whether the shipment has additional insurance.";

        /// <summary>Column comment: insured value amount.</summary>
        public const string InsuredAmount = "Declared value for shipment insurance.";

        /// <summary>Column comment: package weight in kilograms.</summary>
        public const string Weight = "Package weight in kilograms.";

        /// <summary>Column comment: URL to the shipping label PDF.</summary>
        public const string LabelUrl = "URL or path to the generated shipping label PDF.";
    }
}
