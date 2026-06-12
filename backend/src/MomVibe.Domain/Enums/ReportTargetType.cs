namespace MomVibe.Domain.Enums;

/// <summary>
/// What kind of entity an <c>AbuseReport</c> is filed against.
/// </summary>
public enum ReportTargetType
{
    /// <summary>The report targets a user account directly (e.g., from a profile page).</summary>
    User = 0,
    /// <summary>The report targets a specific item listing.</summary>
    Item = 1,
    /// <summary>The report targets an entire conversation thread.</summary>
    MessageThread = 2,
    /// <summary>The report targets a single message inside a thread.</summary>
    Message = 3,
    /// <summary>The report targets a coach / agency business listing.</summary>
    BusinessListing = 4
}
