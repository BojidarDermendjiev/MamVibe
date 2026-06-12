namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using Common;
using Enums;

/// <summary>
/// User-submitted appeal against a specific <see cref="UserModerationLog"/> action.
/// Database-enforced uniqueness allows only one open appeal per moderation event.
/// </summary>
public class ModerationAppeal : BaseEntity
{
    /// <summary>Identifier of the appealing user (FK to ApplicationUser.Id). Must equal <c>UserModerationLog.UserId</c>.</summary>
    [Required]
    public required string UserId { get; set; }

    /// <summary>The moderation log entry being appealed.</summary>
    [Required]
    public Guid ModerationLogId { get; set; }

    /// <summary>The user's free-text statement explaining why the action should be reversed (max 3000 chars).</summary>
    [Required]
    [MaxLength(3000)]
    public required string UserStatement { get; set; }

    /// <summary>Lifecycle status.</summary>
    public AppealStatus Status { get; set; } = AppealStatus.Pending;

    /// <summary>Identifier of the admin who decided the appeal.</summary>
    [MaxLength(450)]
    public string? AdminId { get; set; }

    /// <summary>Optional decision note from the admin (may be surfaced to the user in the decision email).</summary>
    [MaxLength(2000)]
    public string? AdminDecisionNote { get; set; }

    /// <summary>UTC timestamp when the appeal was decided.</summary>
    public DateTime? DecidedAt { get; set; }
}
