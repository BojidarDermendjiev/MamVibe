namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// Immutable evidence row recording that a <see cref="BusinessProfile"/> accepted a specific
/// <see cref="BusinessPolicyVersion"/>. Composite uniqueness on (ProfileId, PolicyVersionId)
/// prevents duplicate acceptances of the same version.
/// </summary>
public class BusinessPolicyAcceptance : BaseEntity
{
    /// <summary>FK to the accepting profile.</summary>
    public Guid BusinessProfileId { get; set; }

    /// <summary>FK to the accepted policy version.</summary>
    public Guid PolicyVersionId { get; set; }

    /// <summary>UTC timestamp of acceptance.</summary>
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Truncated client IP at acceptance time (IPv4 /24, IPv6 /64).</summary>
    [MaxLength(45)]
    public string? Ip { get; set; }

    /// <summary>User-agent header at acceptance time.</summary>
    [MaxLength(512)]
    public string? UserAgent { get; set; }

    /// <summary>Navigation to the accepting profile.</summary>
    public BusinessProfile BusinessProfile { get; set; } = null!;

    /// <summary>Navigation to the accepted version.</summary>
    public BusinessPolicyVersion PolicyVersion { get; set; } = null!;
}
