namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// A published version of the business-vertical platform policy. Each language (en, bg)
/// has at most one row with <see cref="IsCurrent"/>=true at a time — enforced by a partial
/// unique index in the EF configuration. When the current version changes, existing
/// profiles re-trigger the acceptance modal on next dashboard visit.
/// </summary>
public class BusinessPolicyVersion : BaseEntity
{
    /// <summary>Monotonically increasing version number per language.</summary>
    public int Version { get; set; }

    /// <summary>BCP-47 language code of the policy body ("en", "bg").</summary>
    [Required]
    [MaxLength(10)]
    public required string Language { get; set; }

    /// <summary>Short title shown at the top of the acceptance modal.</summary>
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    /// <summary>Markdown body of the policy (rendered inside the modal; ≤32k chars).</summary>
    [Required]
    [MaxLength(32000)]
    public required string BodyMarkdown { get; set; }

    /// <summary>UTC date from which this version is the canonical one for new acceptances.</summary>
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    /// <summary>True exactly when this is the active policy version for its language.</summary>
    public bool IsCurrent { get; set; }
}
