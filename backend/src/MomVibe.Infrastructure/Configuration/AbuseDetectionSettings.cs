namespace MomVibe.Infrastructure.Configuration;

/// <summary>
/// Thresholds for the auto-detection heuristics. Bound from <c>appsettings.json</c> via
/// <c>IOptions&lt;AbuseDetectionSettings&gt;</c>.
/// </summary>
public sealed class AbuseDetectionSettings
{
    public int FailedLoginThreshold { get; set; } = 5;
    public int FailedLoginWindowMinutes { get; set; } = 5;
    public int FailedLoginScore { get; set; } = 40;

    public int MassListingThreshold { get; set; } = 10;
    public int MassListingWindowMinutes { get; set; } = 60;
    public int MassListingScore { get; set; } = 60;

    public int SpamMessageScore { get; set; } = 30;

    /// <summary>Regex word list (each entry is a substring or regex literal). Empty = disabled.</summary>
    public List<string> SpamKeywords { get; set; } = new()
    {
        "western union",
        "money gram",
        "bitcoin",
        "click here",
        "iban\\s*[:=]",
        "(?:\\+|00)?\\d{9,15}",   // raw phone number heuristic (covers off-platform contact attempts)
    };

    public int MultiAccountThreshold { get; set; } = 3;
    public int MultiAccountWindowHours { get; set; } = 24;
    public int MultiAccountScore { get; set; } = 70;
}
