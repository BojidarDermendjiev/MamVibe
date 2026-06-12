namespace MomVibe.Domain.Enums;

/// <summary>Lifecycle of an <c>AbuseReport</c> as it moves through the admin queue.</summary>
public enum ReportStatus
{
    /// <summary>Awaiting first admin look.</summary>
    Pending = 0,
    /// <summary>An admin has claimed the report and is investigating.</summary>
    UnderReview = 1,
    /// <summary>Resolved by taking a moderation action against the target.</summary>
    Resolved = 2,
    /// <summary>Reviewed and dismissed (no action taken — false report or unfounded).</summary>
    Dismissed = 3,
    /// <summary>Closed because another report covers the same target/incident.</summary>
    Duplicate = 4
}
