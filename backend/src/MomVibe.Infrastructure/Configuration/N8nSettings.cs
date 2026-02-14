namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for n8n webhook integration.
/// Bound from the "N8n" section in appsettings.json.
/// </summary>
public class N8nSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string PaymentCompleted { get; set; } = "payment-completed";
    public string PaymentFailed { get; set; } = "payment-failed";
    public string ShipmentCreated { get; set; } = "shipment-created";
    public string ShipmentDelivered { get; set; } = "shipment-delivered";
    public string ShipmentStuck { get; set; } = "shipment-stuck";
    public string UserRegistered { get; set; } = "user-registered";
    public string UserBlocked { get; set; } = "user-blocked";
    public string ItemSold { get; set; } = "item-sold";
    public string NewChatMessage { get; set; } = "new-chat-message";
    public string StaleItems { get; set; } = "stale-items";
    public string DailySummary { get; set; } = "daily-summary";
    public string FeedbackPrompt { get; set; } = "feedback-prompt";
    public string ItemPendingApproval { get; set; } = "item-pending-approval";
    public string ShipmentOutForDelivery { get; set; } = "shipment-out-for-delivery";
}
