namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.Item"/> to keep validation,
/// defaults, and database comments consistent across the codebase.
/// </summary>
public static class ItemConstants
{
    /// <summary>
    /// Length-related constraints for <see cref="MomVibe.Domain.Entities.Item"/> properties.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Minimum length for <c>Title</c>.</summary>
        public const int TitleMin = 2;

        /// <summary>Maximum length for <c>Title</c>.</summary>
        public const int TitleMax = 256;

        /// <summary>Minimum length for <c>Description</c>.</summary>
        public const int DescriptionMin = 10;

        /// <summary>Maximum length for <c>Description</c>.</summary>
        public const int DescriptionMax = 4000;
    }

    /// <summary>
    /// Numeric constraints for count and monetary fields.
    /// </summary>
    public static class Range
    {
        /// <summary>Minimum value for <c>ViewCount</c> and <c>LikeCount</c>.</summary>
        public const int CountMin = 0;

        /// <summary>Minimum allowed price (when provided).</summary>
        public const decimal PriceMin = 0m;
    }

    /// <summary>
    /// Default values for <see cref="MomVibe.Domain.Entities.Item"/>.
    /// </summary>
    public static class Defaults
    {
        /// <summary>Default active state (items start inactive until admin approval).</summary>
        public const bool IsActive = false;

        /// <summary>Default view count.</summary>
        public const int ViewCount = 0;

        /// <summary>Default like count.</summary>
        public const int LikeCount = 0;
    }

    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    /// <remarks>
    /// Use via attributes or fluent configuration to produce descriptive database metadata.
    /// Centralizing these strings keeps database documentation consistent across the codebase.
    /// </remarks>
    public static class Comments
    {
        /// <summary>Column comment: human-readable item title.</summary>
        public const string Title = "Human-readable item title.";

        /// <summary>Column comment: detailed description of the item.</summary>
        public const string Description = "Detailed description of the item.";

        /// <summary>Column comment: foreign key to the item's category.</summary>
        public const string CategoryId = "Foreign key referencing the item's category.";

        /// <summary>Column comment: listing type (e.g., giveaway, sell, request).</summary>
        public const string ListingType = "Listing type (domain-specific enumeration).";

        /// <summary>
        /// Column comment: item price in currency units; null if not applicable (e.g., free or request-type listing).
        /// </summary>
        public const string Price = "Item price in currency units; null if not applicable.";

        /// <summary>Column comment: foreign key to the owning user.</summary>
        public const string UserId = "Foreign key referencing the owning user's identifier.";

        /// <summary>Column comment: whether the listing is active/visible.</summary>
        public const string IsActive = "Indicates whether the listing is active/visible.";

        /// <summary>Column comment: total number of views.</summary>
        public const string ViewCount = "Total number of views for this item.";

        /// <summary>Column comment: total number of likes.</summary>
        public const string LikeCount = "Total number of likes for this item.";
    }
}