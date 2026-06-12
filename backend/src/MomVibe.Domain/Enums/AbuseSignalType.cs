namespace MomVibe.Domain.Enums;

/// <summary>
/// Source of an automatically-detected abuse signal. Signals never auto-enforce —
/// they only flag a user account for admin review via the <c>AbuseSignal</c> queue.
/// </summary>
public enum AbuseSignalType
{
    /// <summary>Threshold of failed login attempts crossed within a sliding window.</summary>
    FailedLoginBurst = 0,
    /// <summary>User created many active listings in a short window (potential mass-spam seller).</summary>
    MassListingCreation = 1,
    /// <summary>Outgoing message matched a configured spam/scam keyword list.</summary>
    SpamKeywordMessage = 2,
    /// <summary>Cumulative user reports against the same target crossed the auto-flag threshold.</summary>
    ReportThreshold = 3,
    /// <summary>Multiple distinct accounts registered from the same IP within a short window.</summary>
    MultiAccountSameIp = 4,
    /// <summary>A device fingerprint already attached to an active <c>BusinessProfile</c> attempted to register a second business account.</summary>
    MultiAccountSameDevice = 5
}
