namespace MomVibe.Application.DTOs.Users;

using Domain.Enums;

/// <summary>
/// DTO for updating user profile settings:
/// - DisplayName, Bio, AvatarUrl: optional personal info updates.
/// - ProfileType: optional change to user profile classification.
/// - LanguagePreference: optional preferred locale/language code.
/// Only provided fields should be applied; unspecified fields remain unchanged.
/// </summary>
public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public ProfileType? ProfileType { get; set; }
    public string? LanguagePreference { get; set; }
    public string? RevolutTag { get; set; }
}
