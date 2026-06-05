namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Common;
using Constants;

/// <summary>
/// Represents a content category that groups items, with a human-readable name and a unique URL-friendly slug.
/// </summary>
/// <remarks>
/// - Uses <see cref="BaseEntity"/> for identity and auditing.
/// - Validation attributes are backed by constants for consistency.
/// - Indexes and column comments are defined in the Infrastructure configuration class.
/// </remarks>
public class Category : BaseEntity
{
    /// <summary>
    /// Human-readable category name.
    /// </summary>
    [Required]
    [MinLength(CategoryConstants.Lengths.NameMin)]
    [MaxLength(CategoryConstants.Lengths.NameMax)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the category.
    /// </summary>
    [MaxLength(CategoryConstants.Lengths.DescriptionMax)]
    public string? Description { get; set; }

    /// <summary>
    /// URL-friendly unique identifier for the category.
    /// </summary>
    [Required]
    [MinLength(CategoryConstants.Lengths.SlugMin)]
    [MaxLength(CategoryConstants.Lengths.SlugMax)]
    [RegularExpression(CategoryConstants.Regex.SlugPattern)]
    public required string Slug { get; set; }

    /// <summary>
    /// Items that belong to this category.
    /// </summary>
    public ICollection<Item> Items { get; set; } = [];
}
