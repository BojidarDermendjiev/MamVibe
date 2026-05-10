namespace MomVibe.Application.DTOs.Users;

using Domain.Enums;

/// <summary>
/// DTO representing a user profile:
/// - Identity: Id, Email, DisplayName.
/// - Classification: ProfileType and Roles.
/// - Profile details: AvatarUrl, Bio, PhoneNumber.
/// - Preferences: LanguagePreference (default: "en").
/// - Status: IsBlocked.
/// - Metadata: CreatedAt.
/// </summary>
public class UserDto
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public required string Id { get; set; }
    /// <summary>Gets or sets the user's email address.</summary>
    public required string Email { get; set; }
    /// <summary>Gets or sets the user's public display name shown on listings and in conversations.</summary>
    public required string DisplayName { get; set; }
    /// <summary>Gets or sets the profile type (Seller, Buyer, etc.).</summary>
    public ProfileType ProfileType { get; set; }
    /// <summary>Gets or sets the URL of the user's profile avatar image.</summary>
    public string? AvatarUrl { get; set; }
    /// <summary>Gets or sets the user's public bio or description.</summary>
    public string? Bio { get; set; }
    /// <summary>Gets or sets the user's contact phone number.</summary>
    public string? PhoneNumber { get; set; }
    /// <summary>Gets or sets the user's preferred interface language (e.g. "en", "bg"; default: "en").</summary>
    public string LanguagePreference { get; set; } = "en";
    /// <summary>Gets or sets the list of roles assigned to the user (e.g. "Admin", "User").</summary>
    public List<string> Roles { get; set; } = [];
    /// <summary>Gets or sets a value indicating whether this user has been blocked by an administrator.</summary>
    public bool IsBlocked { get; set; }
    /// <summary>Gets or sets the timestamp when the user account was created.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Gets or sets the user's Revolut payment tag for peer-to-peer payment links.</summary>
    public string? RevolutTag { get; set; }
    /// <summary>Gets or sets the user's average seller rating; null if no ratings have been received.</summary>
    public double? AverageRating { get; set; }
    /// <summary>Gets or sets the total number of ratings the user has received.</summary>
    public int RatingCount { get; set; }
}
