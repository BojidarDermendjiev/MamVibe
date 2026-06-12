namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// Join row associating a <see cref="DeviceFingerprint"/> with a single
/// <see cref="ApplicationUser"/>. Composite uniqueness on (FingerprintHash, UserId) prevents
/// duplicate links; multiple users on the same hash mark a potential abuse signal that the
/// admin queue surfaces via <c>AbuseSignal</c>.
/// </summary>
public class DeviceFingerprintUser : BaseEntity
{
    /// <summary>FK to <see cref="DeviceFingerprint.Hash"/>.</summary>
    [Required]
    [MaxLength(128)]
    public required string FingerprintHash { get; set; }

    /// <summary>FK to the linked user.</summary>
    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    /// <summary>UTC timestamp when this user was first seen on this device fingerprint.</summary>
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation to the parent fingerprint.</summary>
    public DeviceFingerprint Fingerprint { get; set; } = null!;

    /// <summary>Navigation to the linked user.</summary>
    public ApplicationUser User { get; set; } = null!;
}
