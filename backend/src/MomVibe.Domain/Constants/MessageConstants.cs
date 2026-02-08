namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.Message"/> to keep validation,
/// defaults, and database comments consistent across the codebase.
/// </summary>
public static class MessageConstants
{
    /// <summary>
    /// Length-related constraints for <see cref="MomVibe.Domain.Entities.Message"/> properties.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Minimum length for <c>Content</c>.</summary>
        public const int ContentMin = 1;

        /// <summary>Maximum length for <c>Content</c>.</summary>
        public const int ContentMax = 2000;
    }

    /// <summary>
    /// Default values for <see cref="MomVibe.Domain.Entities.Message"/>.
    /// </summary>
    public static class Defaults
    {
        /// <summary>Default read state for a new message.</summary>
        public const bool IsRead = false;
    }

    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    /// <remarks>
    /// Use via attributes or fluent configuration to produce descriptive database metadata:
    /// - Attribute: <c>[Microsoft.EntityFrameworkCore.Comment(MessageConstants.Comments.Content)]</c>
    /// - Fluent API: <c>builder.Property(m =&gt; m.Content).HasComment(MessageConstants.Comments.Content);</c>
    /// Centralizing these strings keeps database documentation consistent across the codebase.
    /// </remarks>
    public static class Comments
    {
        /// <summary>
        /// Column comment: identifier of the sending user (FK to ApplicationUser.Id).
        /// </summary>
        public const string SenderId = "Identifier of the sending user (FK to ApplicationUser.Id).";

        /// <summary>
        /// Column comment: identifier of the receiving user (FK to ApplicationUser.Id).
        /// </summary>
        public const string ReceiverId = "Identifier of the receiving user (FK to ApplicationUser.Id).";

        /// <summary>
        /// Column comment: textual content of the message.
        /// </summary>
        public const string Content = "Textual content of the message.";

        /// <summary>
        /// Column comment: whether the message has been read by the receiver.
        /// </summary>
        public const string IsRead = "Indicates whether the message has been read by the receiver.";
    }
}