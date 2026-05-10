namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.Category"/> to keep validation
/// and schema comments consistent and maintainable.
/// </summary>
public static class CategoryConstants
{
    /// <summary>
    /// Length-related constraints for <see cref="MomVibe.Domain.Entities.Category"/> properties.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Minimum length for <c>Name</c>.</summary>
        public const int NameMin = 2;

        /// <summary>Maximum length for <c>Name</c>.</summary>
        public const int NameMax = 128;

        /// <summary>Maximum length for <c>Description</c>.</summary>
        public const int DescriptionMax = 1024;

        /// <summary>Minimum length for <c>Slug</c>.</summary>
        public const int SlugMin = 2;

        /// <summary>Maximum length for <c>Slug</c>.</summary>
        public const int SlugMax = 128;
    }

    /// <summary>
    /// Regular-expression patterns used for validation of <see cref="MomVibe.Domain.Entities.Category"/> properties.
    /// </summary>
    public static class Regex
    {
        /// <summary>
        /// Slug pattern: lowercase letters and digits separated by single dashes (no leading/trailing dash).
        /// Example: "healthy-meals", "tips2".
        /// </summary>
        public const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";
    }

    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    /// <remarks>
    /// Use with EF Core comments via attributes or fluent configuration:
    /// - Attribute: <c>[Microsoft.EntityFrameworkCore.Comment(CategoryConstants.Comments.Name)]</c>
    /// - Fluent API: <c>builder.Property(c =&gt; c.Name).HasComment(CategoryConstants.Comments.Name);</c>
    /// Centralizing these strings helps keep database documentation consistent across the codebase.
    /// </remarks>
    public static class Comments
    {
        /// <summary>
        /// Column comment: human-readable category name.
        /// </summary>
        public const string Name = "Human-readable category name.";

        /// <summary>
        /// Column comment: optional description of the category.
        /// </summary>
        public const string Description = "Optional description of the category.";
        
        /// <summary>
        /// Column comment: URL-friendly unique identifier for the category.
        /// </summary>
        public const string Slug = "URL-friendly unique identifier for the category.";
    }
}