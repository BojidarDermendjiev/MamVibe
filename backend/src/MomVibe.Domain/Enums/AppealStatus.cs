namespace MomVibe.Domain.Enums;

/// <summary>Lifecycle of a <c>ModerationAppeal</c> from submission to decision.</summary>
public enum AppealStatus
{
    /// <summary>User submitted the appeal; awaiting admin review.</summary>
    Pending = 0,
    /// <summary>An admin has claimed the appeal and is investigating.</summary>
    UnderReview = 1,
    /// <summary>Admin agreed with the user; the underlying moderation has been cleared.</summary>
    Approved = 2,
    /// <summary>Admin disagreed; the underlying moderation remains in force.</summary>
    Rejected = 3
}
