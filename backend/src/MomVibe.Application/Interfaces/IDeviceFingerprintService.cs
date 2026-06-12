namespace MomVibe.Application.Interfaces;

/// <summary>
/// Hashes FingerprintJS visitor ids and enforces the "one business profile per device"
/// anti-abuse rule. The raw visitor id is never persisted — only its SHA-256 hex hash.
/// </summary>
public interface IDeviceFingerprintService
{
    /// <summary>SHA-256 hex of <paramref name="visitorId"/>. Pure function — no DB access.</summary>
    string HashVisitorId(string visitorId);

    /// <summary>
    /// Returns the user-id of an existing, active <c>BusinessProfile</c> linked to this
    /// fingerprint hash but owned by a user other than <paramref name="currentUserId"/>;
    /// returns null when no such conflict exists.
    /// </summary>
    Task<string?> GetConflictingUserIdAsync(string fingerprintHash, string currentUserId);

    /// <summary>
    /// Idempotently upserts the <c>DeviceFingerprint</c> + <c>DeviceFingerprintUser</c> link
    /// for the given (hash, userId) pair, updating <c>LastSeenAt</c>. Caller is responsible
    /// for the surrounding transaction.
    /// </summary>
    Task UpsertLinkAsync(string fingerprintHash, string userId);

    /// <summary>
    /// Writes an <c>AbuseSignal</c> of type <c>MultiAccountSameDevice</c> for admin review.
    /// Score is fixed at 60 (above the auto-flag threshold but below auto-enforce).
    /// </summary>
    Task EmitDuplicateSignalAsync(string fingerprintHash, string currentUserId, string conflictingUserId);

    /// <summary>
    /// Truncates an IPv4 address to /24 (drops the last octet) for GDPR-proportionate storage.
    /// IPv6 is truncated to /64. Returns the input unchanged if it is not a recognisable IP.
    /// </summary>
    string TruncateIpForStorage(string? ipAddress);
}
