namespace MomVibe.Domain.Enums;

/// <summary>
/// Represents the outcome of AI content moderation for an item listing.
/// </summary>
public enum AiModerationStatus
{
    /// <summary>AI screening has not run yet (e.g. API key not configured).</summary>
    NotScreened = 0,

    /// <summary>AI approved with high confidence — item was made active automatically.</summary>
    AutoApproved = 1,

    /// <summary>AI recommends human review before approval.</summary>
    NeedsReview = 2,

    /// <summary>AI flagged the listing as potentially inappropriate or spam.</summary>
    FlaggedForReview = 3
}
