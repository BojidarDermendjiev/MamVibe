namespace MomVibe.Domain.Enums;

/// <summary>
/// Graded moderation level applied to a user account.
/// </summary>
/// <remarks>
/// Levels escalate in severity: None &lt; Warned &lt; Restricted &lt; Suspended &lt; Banned.
/// Warned is informational only — no behavior block. Restricted is fully read-only.
/// Suspended is time-bounded (auto-cleared when <c>ApplicationUser.ModerationExpiresAt</c>
/// elapses). Banned is permanent until manually revoked or an appeal is approved.
/// </remarks>
public enum UserModerationLevel
{
    /// <summary>No moderation applied. Default for healthy accounts.</summary>
    None = 0,
    /// <summary>User notified of a policy concern. No restrictions on actions.</summary>
    Warned = 1,
    /// <summary>Fully read-only mode. All write actions (list, message, offer, buy, like, follow, save-search, rate, review) return 403. Browsing remains available.</summary>
    Restricted = 2,
    /// <summary>Temporary suspension. User cannot log in (refresh tokens revoked, login refused) until <c>ModerationExpiresAt</c>.</summary>
    Suspended = 3,
    /// <summary>Permanent ban. Same enforcement as Suspended but with no expiry.</summary>
    Banned = 4
}
