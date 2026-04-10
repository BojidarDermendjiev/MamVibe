namespace MomVibe.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Enums;
using Constants;

/// <summary>
/// Application user entity extending <see cref="IdentityUser"/> with domain-specific profile data
/// and navigation properties.
/// </summary>
/// <remarks>
/// - Validation attributes are backed by constants to keep constraints consistent and maintainable.
/// - EF Core <see cref="CommentAttribute"/> is used to generate descriptive database column comments.
/// - Timestamps are recommended to be handled in UTC.
/// </remarks>
public class ApplicationUser : IdentityUser
{

    /// <summary>
    /// Public display name shown to other users.
    /// </summary>
    [Required]
    [MinLength(ApplicationUserConstants.Lengths.DisplayNameMin)]
    [MaxLength(ApplicationUserConstants.Lengths.DisplayNameMax)]
    [Comment(ApplicationUserConstants.Comments.DisplayName)]
    public required string DisplayName { get; set; }

    /// <summary>
    /// Type/category of the user's profile.
    /// </summary>
    [Comment(ApplicationUserConstants.Comments.ProfileType)]
    public ProfileType ProfileType { get; set; }

    /// <summary>
    /// Absolute URL to the user's avatar image.
    /// </summary>
    [Url]
    [MaxLength(ApplicationUserConstants.Lengths.AvatarUrlMax)]
    [Comment(ApplicationUserConstants.Comments.AvatarUrl)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Indicates whether the account is blocked from interacting.
    /// </summary>
    [Comment(ApplicationUserConstants.Comments.IsBlocked)]
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Short biography provided by the user.
    /// </summary>
    [MaxLength(ApplicationUserConstants.Lengths.BioMax)]
    [Comment(ApplicationUserConstants.Comments.Bio)]
    public string? Bio { get; set; } = string.Empty;

    /// <summary>
    /// Preferred language or locale (e.g., "en" or "en-US").
    /// </summary>
    [Required]
    [MaxLength(ApplicationUserConstants.Lengths.LanguageCodeMax)]
    [Comment(ApplicationUserConstants.Comments.LanguagePreference)]
    public string LanguagePreference { get; set; } = "en";

    /// <summary>
    /// UTC timestamp when the user account was created.
    /// </summary>
    [Comment(ApplicationUserConstants.Comments.CreatedAt)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IBAN for receiving card payments.
    /// </summary>
    [MaxLength(ApplicationUserConstants.Lengths.IbanMax)]
    [Comment(ApplicationUserConstants.Comments.Iban)]
    public string? Iban { get; set; }

    /// <summary>
    /// Expo push notification token for the user's mobile device.
    /// Null when the user has not granted push notification permission or has not used the mobile app.
    /// </summary>
    [MaxLength(200)]
    public string? ExpoPushToken { get; set; }

    /// <summary>
    /// Items created by this user.
    /// </summary>
    public ICollection<Item> Items { get; set; } = [];
    
    /// <summary>
    /// Items liked by this user.
    /// </summary>
    public ICollection<Like> Likes { get; set; } = [];

    /// <summary>
    /// Messages sent by this user.
    /// </summary>
    public ICollection<Message> SentMessages { get; set; } = [];

    /// <summary>
    /// Messages received by this user.
    /// </summary>
    public ICollection<Message> ReceivedMessages { get; set; } = [];

    /// <summary>
    /// Refresh tokens issued to this user.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    /// <summary>
    /// The user's platform wallet. Created automatically on first wallet interaction.
    /// </summary>
    public Wallet? Wallet { get; set; }
}
