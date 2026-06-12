namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using Common;
using Enums;

/// <summary>
/// User-submitted report flagging another user, item, message, or thread for admin review.
/// Aggregated per <see cref="TargetUserId"/> so the admin queue can show a unified history per user.
/// </summary>
public class AbuseReport : BaseEntity
{
    /// <summary>Identifier of the user who submitted the report (FK to ApplicationUser.Id).</summary>
    [Required]
    public required string ReporterId { get; set; }

    /// <summary>What kind of entity is being reported.</summary>
    public ReportTargetType TargetType { get; set; }

    /// <summary>
    /// Heterogeneous identifier for the target. For <see cref="ReportTargetType.User"/> this is
    /// the Identity user id. For <see cref="ReportTargetType.Item"/> and
    /// <see cref="ReportTargetType.Message"/> this is a canonical Guid string. For
    /// <see cref="ReportTargetType.MessageThread"/> this is the normalised
    /// <c>min(uid1)|max(uid2)</c> thread key. The service layer enforces parseability based on type.
    /// </summary>
    [Required]
    [MaxLength(450)]
    public required string TargetId { get; set; }

    /// <summary>
    /// Denormalised owner of the target — populated by the service from the underlying entity.
    /// Used by the admin queue to group reports by the user ultimately responsible. For
    /// <see cref="ReportTargetType.User"/> this equals <see cref="TargetId"/>.
    /// </summary>
    [Required]
    [MaxLength(450)]
    public required string TargetUserId { get; set; }

    /// <summary>Categorised reason chosen by the reporter from the standard reason list.</summary>
    public ModerationReason Reason { get; set; }

    /// <summary>Free-text description supplied by the reporter (10–2000 chars, FluentValidation enforced).</summary>
    [Required]
    [MaxLength(2000)]
    public required string Description { get; set; }

    /// <summary>Lifecycle status of the report.</summary>
    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    /// <summary>Identifier of the admin who resolved the report.</summary>
    [MaxLength(450)]
    public string? ResolvedByAdminId { get; set; }

    /// <summary>UTC timestamp when the report was resolved.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Optional admin note explaining the resolution.</summary>
    [MaxLength(1000)]
    public string? ResolutionNote { get; set; }

    /// <summary>When the resolution included a moderation action, the resulting <c>UserModerationLog</c> id.</summary>
    public Guid? ResultingModerationLogId { get; set; }
}
