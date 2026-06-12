namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// A parent's comment on a <see cref="BusinessListing"/>. Single-level threading is supported
/// via the self-referential <see cref="ParentCommentId"/> — replies cannot themselves be replied to,
/// which keeps the UI flat and the moderation surface small.
/// </summary>
public class BusinessListingComment : BaseEntity
{
    /// <summary>FK to the listing being commented on.</summary>
    public Guid ListingId { get; set; }

    /// <summary>Identifier of the commenting user (FK to ApplicationUser.Id).</summary>
    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    /// <summary>Comment body (≤1000 chars).</summary>
    [Required]
    [MaxLength(1000)]
    public required string Body { get; set; }

    /// <summary>Optional FK to a parent comment when this comment is a reply.</summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>True when admin has hidden the comment.</summary>
    public bool IsHidden { get; set; }

    /// <summary>Admin-supplied note explaining why the comment was hidden.</summary>
    [MaxLength(500)]
    public string? HiddenReason { get; set; }

    /// <summary>Navigation to the listing.</summary>
    public BusinessListing Listing { get; set; } = null!;

    /// <summary>Navigation to the commenting user.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Navigation to the parent comment, when present.</summary>
    public BusinessListingComment? ParentComment { get; set; }
}
