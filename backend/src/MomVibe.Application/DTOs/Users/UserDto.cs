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
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public ProfileType ProfileType { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string LanguagePreference { get; set; } = "en";
    public List<string> Roles { get; set; } = [];
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RevolutTag { get; set; }
    public double? AverageRating { get; set; }
    public int RatingCount { get; set; }
}
