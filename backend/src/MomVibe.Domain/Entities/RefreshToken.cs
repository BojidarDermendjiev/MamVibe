namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Common;
using Constants;

/// <summary>
/// Represents a refresh token associated with a user, including expiration, optional revocation,
/// and rotation metadata.
/// </summary>
/// <remarks>
/// - Inherits <see cref="BaseEntity"/> for identity and audit fields.
/// - Validation attributes use centralized constants for consistency.
/// - Computed properties (<c>IsExpired</c>, <c>IsRevoked</c>, <c>IsActive</c>) are not mapped to the database.
/// - Indexes and column comments are defined in the Infrastructure configuration class.
/// </remarks>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Raw or hashed refresh token string.
    /// </summary>
    [Required]
    [MaxLength(RefreshTokenConstants.Lengths.TokenMax)]
    public required string Token { get; set; }

    /// <summary>
    /// Identifier of the user to whom the token belongs (FK to ApplicationUser.Id).
    /// </summary>
    [Required]
    public required string UserId { get; set; }

    /// <summary>
    /// UTC timestamp when the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// UTC timestamp when the token was revoked; null if still valid.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Token that replaced this one in a rotation flow; null if not replaced.
    /// </summary>
    [MaxLength(RefreshTokenConstants.Lengths.ReplacedByTokenMax)]
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Indicates whether the token is expired relative to the current UTC time.
    /// </summary>
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Indicates whether the token has been revoked.
    /// </summary>
    [NotMapped]
    public bool IsRevoked => RevokedAt is not null;

    /// <summary>
    /// Indicates whether the token is currently active (not expired and not revoked).
    /// </summary>
    [NotMapped]
    public bool IsActive => !IsExpired && !IsRevoked;

    /// <summary>
    /// Navigation to the owning user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}