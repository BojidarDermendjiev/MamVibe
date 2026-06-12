namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>
/// Lightweight projection used by the public browse list. Drops description, schedule,
/// and contact fields to keep the payload small for grid rendering.
/// </summary>
public class BusinessListingSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; }
    public string City { get; set; } = string.Empty;
    public short? AgeFromMonths { get; set; }
    public short? AgeToMonths { get; set; }
    public decimal? PriceFromEur { get; set; }
    public string? CoverPhotoUrl { get; set; }

    /// <summary>Public business name shown on the card.</summary>
    public string BusinessDisplayName { get; set; } = string.Empty;

    /// <summary>0 / 50 / 100 — used to render a "Featured" or "Premium" badge.</summary>
    public int RankBoost { get; set; }

    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
