namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Server-side record of a device fingerprint seen during business-profile registration.
/// The primary key is the SHA-256 hash of the FingerprintJS visitorId — the raw value is
/// never stored — so the table is itself the canonical anti-abuse index.
/// </summary>
public class DeviceFingerprint
{
    /// <summary>SHA-256 hex of the visitorId (primary key).</summary>
    [Key]
    [MaxLength(128)]
    public required string Hash { get; set; }

    /// <summary>UTC timestamp when this fingerprint was first observed.</summary>
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the most recent observation.</summary>
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>Cached count of users linked to this fingerprint (driven by <see cref="DeviceFingerprintUser"/>).</summary>
    public int LinkedUserCount { get; set; }

    /// <summary>True once an admin has reviewed and acknowledged a flagged duplicate-account signal.</summary>
    public bool ReviewedByAdmin { get; set; }

    /// <summary>Join rows associating this fingerprint with users that have used it.</summary>
    public ICollection<DeviceFingerprintUser> LinkedUsers { get; set; } = [];
}
