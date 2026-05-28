namespace MomVibe.Application.DTOs.Bundles;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Payload for creating a new seller bundle.
/// </summary>
public class CreateBundleDto
{
    /// <summary>Gets or sets the human-readable bundle title (max 150 characters).</summary>
    [Required, MaxLength(150)]
    public required string Title { get; set; }

    /// <summary>Gets or sets the optional description (max 1000 characters).</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the discounted bundle price (must be positive).</summary>
    [Required, Range(0.01, 999999)]
    public decimal Price { get; set; }

    /// <summary>Gets or sets the list of item IDs to include (2–10 items required).</summary>
    [Required, MinLength(2), MaxLength(10)]
    public required List<Guid> ItemIds { get; set; }
}
