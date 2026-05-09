namespace MomVibe.Application.DTOs.Users;

using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.Constants;

/// <summary>
/// Payload used when an authenticated user updates their own profile.
/// All fields are optional; only non-null values are applied.
/// </summary>
public class UpdateProfileDto
{
    /// <summary>Gets or sets the new display name for the user's profile.</summary>
    [MaxLength(ApplicationUserConstants.Lengths.DisplayNameMax)]
    public string? DisplayName { get; set; }

    /// <summary>Gets or sets the new short biography for the user's profile.</summary>
    [MaxLength(ApplicationUserConstants.Lengths.BioMax)]
    public string? Bio { get; set; }

    /// <summary>Gets or sets the URL of the new profile avatar image.</summary>
    [MaxLength(ApplicationUserConstants.Lengths.AvatarUrlMax)]
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the updated profile type (e.g., parent, professional).</summary>
    public ProfileType? ProfileType { get; set; }

    /// <summary>Gets or sets the BCP-47 language code for the user's preferred UI language.</summary>
    [MaxLength(ApplicationUserConstants.Lengths.LanguageCodeMax)]
    public string? LanguagePreference { get; set; }

    /// <summary>Gets or sets the user's Revolut payment tag for peer-to-peer transactions.</summary>
    [MaxLength(ApplicationUserConstants.Lengths.RevolutTagMax)]
    public string? RevolutTag { get; set; }
}
