namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using Common;
using Enums;

/// <summary>
/// Immutable record of a moderation action applied to a user account (Warn, Restrict, Suspend, Ban,
/// or revoke). Written at the moment the action is taken; never updated after creation.
/// </summary>
/// <remarks>
/// Mirrors <see cref="ItemModerationLog"/> in pattern but tracks the user-level action surface
/// (graded level transitions) rather than item moderation outcomes.
/// </remarks>
public class UserModerationLog : BaseEntity
{
    /// <summary>Identifier of the user the action was applied to (FK to ApplicationUser.Id).</summary>
    [Required]
    public required string UserId { get; set; }

    /// <summary>Identifier of the admin who applied the action. The literal value <c>"system"</c> is used for auto-expiry events written by <c>ModerationExpiryService</c>.</summary>
    [Required]
    public required string AdminId { get; set; }

    /// <summary>Display name of the admin at the time of the action — denormalised for read-only audit clarity.</summary>
    [Required]
    [MaxLength(100)]
    public required string AdminDisplayName { get; set; }

    /// <summary>Moderation level the user was at before this action.</summary>
    public UserModerationLevel PreviousLevel { get; set; }

    /// <summary>Moderation level applied by this action.</summary>
    public UserModerationLevel NewLevel { get; set; }

    /// <summary>Categorised reason driving the action.</summary>
    public ModerationReason Reason { get; set; }

    /// <summary>User-facing reason shown in the banner and email. Localised by caller before persistence; max 500 chars.</summary>
    [Required]
    [MaxLength(500)]
    public required string PublicReason { get; set; }

    /// <summary>Admin-only internal note (context, links to reports, etc.). Never exposed to the user.</summary>
    [MaxLength(2000)]
    public string? InternalNote { get; set; }

    /// <summary>UTC expiry — set for temporary <see cref="UserModerationLevel.Suspended"/> actions, null otherwise.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Optional link to the <c>AbuseReport</c> that triggered this action.</summary>
    public Guid? RelatedReportId { get; set; }

    /// <summary>Optional link to the <c>ModerationAppeal</c> whose decision produced this action.</summary>
    public Guid? RelatedAppealId { get; set; }
}
