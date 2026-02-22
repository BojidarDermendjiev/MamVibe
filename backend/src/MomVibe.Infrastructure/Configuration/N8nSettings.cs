namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for n8n webhook integration.
/// Bound from the "N8n" section in appsettings.json.
/// </summary>
public class N8nSettings
{
    /// <summary>
    /// The base URL for the n8n webhook service.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether n8n webhook integration is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Webhook endpoint for payment completion events.
    /// </summary>
    public string PaymentCompleted { get; set; } = "payment-completed";

    /// <summary>
    /// Webhook endpoint for payment failure events.
    /// </summary>
    public string PaymentFailed { get; set; } = "payment-failed";

    /// <summary>
    /// Webhook endpoint for shipment creation events.
    /// </summary>
    public string ShipmentCreated { get; set; } = "shipment-created";

    /// <summary>
    /// Webhook endpoint for shipment delivery events.
    /// </summary>
    public string ShipmentDelivered { get; set; } = "shipment-delivered";

    /// <summary>
    /// Webhook endpoint for stuck shipment events.
    /// </summary>
    public string ShipmentStuck { get; set; } = "shipment-stuck";

    /// <summary>
    /// Webhook endpoint for user registration events.
    /// </summary>
    public string UserRegistered { get; set; } = "user-registered";

    /// <summary>
    /// Webhook endpoint for user blocking events.
    /// </summary>
    public string UserBlocked { get; set; } = "user-blocked";

    /// <summary>
    /// Webhook endpoint for item sold events.
    /// </summary>
    public string ItemSold { get; set; } = "item-sold";

    /// <summary>
    /// Webhook endpoint for new chat message events.
    /// </summary>
    public string NewChatMessage { get; set; } = "new-chat-message";

    /// <summary>
    /// Webhook endpoint for stale items notification events.
    /// </summary>
    public string StaleItems { get; set; } = "stale-items";

    /// <summary>
    /// Webhook endpoint for daily summary report events.
    /// </summary>
    public string DailySummary { get; set; } = "daily-summary";

    /// <summary>
    /// Webhook endpoint for feedback prompt events.
    /// </summary>
    public string FeedbackPrompt { get; set; } = "feedback-prompt";

    /// <summary>
    /// Webhook endpoint for item pending approval events.
    /// </summary>
    public string ItemPendingApproval { get; set; } = "item-pending-approval";

    /// <summary>
    /// Webhook endpoint for shipment out for delivery events.
    /// </summary>
    public string ShipmentOutForDelivery { get; set; } = "shipment-out-for-delivery";
}
