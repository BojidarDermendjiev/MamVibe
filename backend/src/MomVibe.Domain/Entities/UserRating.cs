namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;

/// <summary>
/// A star rating left by a buyer for a seller after a completed purchase.
/// One rating per purchase request (enforced via unique index on PurchaseRequestId).
/// </summary>
public class UserRating : BaseEntity
{
    [Required]
    public required string RaterId { get; set; }

    [Required]
    public required string RatedUserId { get; set; }

    public Guid PurchaseRequestId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    public ApplicationUser Rater { get; set; } = null!;
    public ApplicationUser RatedUser { get; set; } = null!;
    public PurchaseRequest PurchaseRequest { get; set; } = null!;
}
