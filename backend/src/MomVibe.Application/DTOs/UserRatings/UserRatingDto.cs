namespace MomVibe.Application.DTOs.UserRatings;

/// <summary>
/// Read-only projection of a <see cref="Domain.Entities.UserRating"/> returned by the API.
/// </summary>
public class UserRatingDto
{
    /// <summary>Gets or sets the unique identifier of the rating.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who submitted the rating.</summary>
    public string RaterId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the user who submitted the rating.</summary>
    public string? RaterDisplayName { get; set; }

    /// <summary>Gets or sets the avatar URL of the user who submitted the rating.</summary>
    public string? RaterAvatarUrl { get; set; }

    /// <summary>Gets or sets the identifier of the user who received the rating.</summary>
    public string RatedUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the purchase request that triggered this rating.</summary>
    public Guid PurchaseRequestId { get; set; }

    /// <summary>Gets or sets the numeric rating on a 1–5 scale.</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets an optional comment accompanying the rating.</summary>
    public string? Comment { get; set; }

    /// <summary>Gets or sets the UTC date and time when the rating was submitted.</summary>
    public DateTime CreatedAt { get; set; }
}
