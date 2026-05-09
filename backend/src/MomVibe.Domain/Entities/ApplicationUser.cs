namespace MomVibe.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Enums;
using Constants;

/// <summary>
/// Represents a registered user of the MomVibe platform, extending ASP.NET Core Identity
/// with application-specific profile and preference data.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Gets or sets the publicly visible display name of the user.</summary>
    [Required]
    [MinLength(ApplicationUserConstants.Lengths.DisplayNameMin)]
    [MaxLength(ApplicationUserConstants.Lengths.DisplayNameMax)]
    [Comment(ApplicationUserConstants.Comments.DisplayName)]
    public required string DisplayName { get; set; }

    /// <summary>Gets or sets the profile type that describes the user's primary role (e.g., parent, professional).</summary>
    [Comment(ApplicationUserConstants.Comments.ProfileType)]
    public ProfileType ProfileType { get; set; }

    /// <summary>Gets or sets the URL of the user's profile avatar image.</summary>
    [Url]
    [MaxLength(ApplicationUserConstants.Lengths.AvatarUrlMax)]
    [Comment(ApplicationUserConstants.Comments.AvatarUrl)]
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the user has been blocked by an administrator.</summary>
    [Comment(ApplicationUserConstants.Comments.IsBlocked)]
    public bool IsBlocked { get; set; }

    /// <summary>Gets or sets a short biography or description written by the user.</summary>
    [MaxLength(ApplicationUserConstants.Lengths.BioMax)]
    [Comment(ApplicationUserConstants.Comments.Bio)]
    public string? Bio { get; set; } = string.Empty;

    /// <summary>Gets or sets the BCP-47 language code representing the user's preferred UI language (e.g., "en", "bg").</summary>
    [Required]
    [MaxLength(ApplicationUserConstants.Lengths.LanguageCodeMax)]
    [Comment(ApplicationUserConstants.Comments.LanguagePreference)]
    public string LanguagePreference { get; set; } = "en";

    /// <summary>Gets or sets the UTC date and time when the user account was created.</summary>
    [Comment(ApplicationUserConstants.Comments.CreatedAt)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the user's Revolut payment tag used for peer-to-peer payments.</summary>
    [MaxLength(ApplicationUserConstants.Lengths.RevolutTagMax)]
    [Comment(ApplicationUserConstants.Comments.RevolutTag)]
    public string? RevolutTag { get; set; }

    /// <summary>Gets or sets the Expo push notification token for sending mobile push notifications.</summary>
    [MaxLength(200)]
    public string? ExpoPushToken { get; set; }

    /// <summary>Gets the collection of marketplace items listed by the user.</summary>
    public ICollection<Item> Items { get; set; } = [];

    /// <summary>Gets the collection of items the user has liked.</summary>
    public ICollection<Like> Likes { get; set; } = [];

    /// <summary>Gets the collection of messages sent by the user.</summary>
    public ICollection<Message> SentMessages { get; set; } = [];

    /// <summary>Gets the collection of messages received by the user.</summary>
    public ICollection<Message> ReceivedMessages { get; set; } = [];

    /// <summary>Gets the collection of active refresh tokens issued to the user.</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    /// <summary>Gets the collection of doctor reviews submitted by the user.</summary>
    public ICollection<DoctorReview> DoctorReviews { get; set; } = [];

    /// <summary>Gets the collection of child-friendly places submitted by the user.</summary>
    public ICollection<ChildFriendlyPlace> ChildFriendlyPlaces { get; set; } = [];

    /// <summary>Gets the collection of ratings the user has given to other users.</summary>
    public ICollection<UserRating> RatingsGiven { get; set; } = [];

    /// <summary>Gets the collection of ratings the user has received from other users.</summary>
    public ICollection<UserRating> RatingsReceived { get; set; } = [];
}
