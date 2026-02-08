namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.Like"/> to keep documentation
/// and database comments consistent across the codebase.
/// </summary>
public static class LikeConstants
{
    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    /// <remarks>
    /// Use via attributes or fluent configuration to produce descriptive database metadata:
    /// - Attribute: <c>[Microsoft.EntityFrameworkCore.Comment(LikeConstants.Comments.UserId)]</c>
    /// - Fluent API: <c>builder.Property(l =&gt; l.UserId).HasComment(LikeConstants.Comments.UserId);</c>
    /// Centralizing these strings keeps database documentation consistent across the codebase.
    /// </remarks>
    public static class Comments
    {
        /// <summary>
        /// Column comment: identifier of the user who liked the item (FK to ApplicationUser.Id).
        /// </summary>
        public const string UserId = "Identifier of the user who liked the item (FK to ApplicationUser.Id).";

        /// <summary>
        /// Column comment: foreign key referencing the liked item.
        /// </summary>
        public const string ItemId = "Foreign key referencing the liked item.";
    }
}