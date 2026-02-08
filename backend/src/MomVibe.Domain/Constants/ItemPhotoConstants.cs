namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.ItemPhoto"/> to keep validation,
/// defaults, and database comments consistent across the codebase.
/// </summary>
public static class ItemPhotoConstants
{
    /// <summary>
    /// Length-related constraints for <see cref="MomVibe.Domain.Entities.ItemPhoto"/> properties.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Maximum length for <c>Url</c>.</summary>
        public const int UrlMax = 2048;
    }

    /// <summary>
    /// Range and default constraints.
    /// </summary>
    public static class Range
    {
        /// <summary>Minimum allowed display order (non-negative).</summary>
        public const int DisplayOrderMin = 0;
    }

    /// <summary>
    /// Default values for <see cref="MomVibe.Domain.Entities.ItemPhoto"/>.
    /// </summary>
    public static class Defaults
    {
        /// <summary>Default display order for newly added photos.</summary>
        public const int DisplayOrder = 0;
    }

    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    /// <remarks>
    /// Use via attributes or fluent configuration to produce descriptive database metadata:
    /// - Attribute: <c>[Microsoft.EntityFrameworkCore.Comment(ItemPhotoConstants.Comments.Url)]</c>
    /// - Fluent API: <c>builder.Property(p =&gt; p.Url).HasComment(ItemPhotoConstants.Comments.Url);</c>
    /// Centralizing these strings keeps database documentation consistent across the codebase.
    /// </remarks>
    public static class Comments
    {
        /// <summary>
        /// Column comment: absolute URL to the photo resource.
        /// </summary>
        public const string Url = "Absolute URL to the photo resource.";

        /// <summary>
        /// Column comment: foreign key referencing the owning item.
        /// </summary>
        public const string ItemId = "Foreign key referencing the owning item.";

        /// <summary>
        /// Column comment: zero-based display order among the item's photos.
        /// </summary>
        public const string DisplayOrder = "Zero-based display order among the item's photos.";
    }
}