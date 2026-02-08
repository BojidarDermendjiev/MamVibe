namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.Feedback"/> to keep validation,
/// documentation, and database constraints consistent and maintainable.
/// </summary>
public static class FeedbackConstants
{
    /// <summary>
    /// Length-related constraints for <see cref="MomVibe.Domain.Entities.Feedback"/> properties.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Minimum length for <c>Content</c>.</summary>
        public const int ContentMin = 10;

        /// <summary>Maximum length for <c>Content</c>.</summary>
        public const int ContentMax = 2000;
    }

    /// <summary>
    /// Range constraints for numeric properties.
    /// </summary>
    public static class Range
    {
        /// <summary>Minimum allowed rating value.</summary>
        public const int RatingMin = 1;

        /// <summary>Maximum allowed rating value.</summary>
        public const int RatingMax = 5;
    }

    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    /// <remarks>
    /// Use with EF Core comments via attributes or fluent configuration to produce descriptive database metadata:
    /// - Attribute: <c>[Microsoft.EntityFrameworkCore.Comment(FeedbackConstants.Comments.Rating)]</c>
    /// - Fluent: <c>builder.Property(f =&gt; f.Rating).HasComment(FeedbackConstants.Comments.Rating);</c>
    /// Centralizing these strings keeps database documentation consistent across the codebase.
    /// </remarks>
    public static class Comments
    {
        /// <summary>
        /// Column comment: identifier of the user who submitted the feedback (FK to ApplicationUser.Id).
        /// </summary>
        public const string UserId = "Identifier of the user who submitted the feedback (FK to ApplicationUser.Id).";

        /// <summary>
        /// Column comment: feedback rating from 1 (lowest) to 5 (highest).
        /// </summary>
        public const string Rating = "Feedback rating from 1 (lowest) to 5 (highest).";

        /// <summary>
        /// Column comment: category/type of the feedback (e.g., bug, feature request, general).
        /// </summary>
        public const string Category = "Category/type of the feedback (e.g., bug, feature request, general).";

        /// <summary>
        /// Column comment: textual content of the feedback.
        /// </summary>
        public const string Content = "Textual content of the feedback.";

        /// <summary>
        /// Column comment: whether the user consents to being contacted regarding this feedback.
        /// </summary>
        public const string IsContactable = "Whether the user consents to being contacted regarding this feedback.";
    }
}