namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// A star rating left by a buyer for a seller after a completed purchase.
/// One rating per purchase request (enforced via unique index on PurchaseRequestId).
/// </summary>
public class UserRating : BaseEntity
{
    /// <summary>Identity of the user who submitted the rating.</summary>
    [Required]
    public required string RaterId { get; set; }

    /// <summary>Identity of the user who received the rating.</summary>
    [Required]
    public required string RatedUserId { get; set; }

    /// <summary>The purchase request this rating is associated with.</summary>
    public Guid PurchaseRequestId { get; set; }

    /// <summary>Star rating value between 1 (worst) and 5 (best).</summary>
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>Optional written feedback accompanying the star rating.</summary>
    [MaxLength(500)]
    public string? Comment { get; set; }

    /// <summary>Navigation property to the user who submitted the rating.</summary>
    public ApplicationUser Rater { get; set; } = null!;

    /// <summary>Navigation property to the user who received the rating.</summary>
    public ApplicationUser RatedUser { get; set; } = null!;

    /// <summary>Navigation property to the associated purchase request.</summary>
    public PurchaseRequest PurchaseRequest { get; set; } = null!;
}
