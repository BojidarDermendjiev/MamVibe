namespace MomVibe.Application.DTOs.UserRatings;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Payload used when a buyer submits a rating for a seller after completing a purchase.
/// </summary>
public class CreateUserRatingDto
{
    /// <summary>Gets or sets the numeric rating on a 1–5 scale.</summary>
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>Gets or sets an optional comment accompanying the rating.</summary>
    [MaxLength(500)]
    public string? Comment { get; set; }
}
