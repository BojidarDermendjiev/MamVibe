namespace MomVibe.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Common;
using Constants;

/// <summary>
/// Represents a content category that groups items, with a human-readable name and a unique URL-friendly slug.
/// </summary>
/// <remarks>
/// - Uses <see cref="BaseEntity"/> for identity and auditing.
/// - Validation attributes are backed by constants for consistency.
/// - EF Core <see cref="CommentAttribute"/> provides descriptive DB column comments.
/// - A unique index on <c>Slug</c> prevents duplicates at the database layer.
/// </remarks>
[Index(nameof(Slug), IsUnique = true)]
public class Category : BaseEntity
{

    /// <summary>
    /// Human-readable category name.
    /// </summary>
    [Required]
    [MinLength(CategoryConstants.Lengths.NameMin)]
    [MaxLength(CategoryConstants.Lengths.NameMax)]
    [Comment(CategoryConstants.Comments.Name)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the category.
    /// </summary>
    [MaxLength(CategoryConstants.Lengths.DescriptionMax)]
    [Comment(CategoryConstants.Comments.Description)]
    public string? Description { get; set; }

    /// <summary>
    /// URL-friendly unique identifier for the category.
    /// </summary>
    [Required]
    [MinLength(CategoryConstants.Lengths.SlugMin)]
    [MaxLength(CategoryConstants.Lengths.SlugMax)]
    [RegularExpression(CategoryConstants.Regex.SlugPattern)]
    [Comment(CategoryConstants.Comments.Slug)]
    public required string Slug { get; set; }

    /// <summary>
    /// Items that belong to this category.
    /// </summary>
    public ICollection<Item> Items { get; set; } = [];
}
