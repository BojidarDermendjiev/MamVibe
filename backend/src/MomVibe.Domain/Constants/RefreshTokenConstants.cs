namespace MomVibe.Domain.Constants;

/// <summary>
/// Centralized constants for <see cref="MomVibe.Domain.Entities.RefreshToken"/> to keep validation,
/// defaults, and database comments consistent across the codebase.
/// </summary>
public static class RefreshTokenConstants
{
    /// <summary>
    /// Length-related constraints for <see cref="MomVibe.Domain.Entities.RefreshToken"/> properties.
    /// </summary>
    public static class Lengths
    {
        /// <summary>Maximum length for <c>Token</c>.</summary>
        public const int TokenMax = 512;

        /// <summary>Maximum length for <c>ReplacedByToken</c>.</summary>
        public const int ReplacedByTokenMax = 512;
    }

    /// <summary>
    /// Database column comments for EF Core schema generation.
    /// </summary>
    /// <remarks>
    /// Use via attributes or fluent configuration to produce descriptive database metadata:
    /// - Attribute: <c>[Microsoft.EntityFrameworkCore.Comment(RefreshTokenConstants.Comments.Token)]</c>
    /// - Fluent API: <c>builder.Property(t =&gt; t.Token).HasComment(RefreshTokenConstants.Comments.Token);</c>
    /// Centralizing these strings keeps database documentation consistent across the codebase.
    /// </remarks>
    public static class Comments
    {
        /// <summary>
        /// Column comment: raw or hashed refresh token string.
        /// </summary>
        public const string Token = "Raw or hashed refresh token string.";

        /// <summary>
        /// Column comment: identifier of the user to whom the token belongs (FK to ApplicationUser.Id).
        /// </summary>
        public const string UserId = "Identifier of the user to whom the token belongs (FK to ApplicationUser.Id).";

        /// <summary>
        /// Column comment: UTC timestamp when the token expires.
        /// </summary>
        public const string ExpiresAt = "UTC timestamp when the token expires.";

        /// <summary>
        /// Column comment: UTC timestamp when the token was revoked; null if still valid.
        /// </summary>
        public const string RevokedAt = "UTC timestamp when the token was revoked; null if still valid.";

        /// <summary>
        /// Column comment: token that replaced this one in a rotation flow; null if not replaced.
        /// </summary>
        public const string ReplacedByToken = "Token that replaced this one in a rotation flow; null if not replaced.";
    }
}