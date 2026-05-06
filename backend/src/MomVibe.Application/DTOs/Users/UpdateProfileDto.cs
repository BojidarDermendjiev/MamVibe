namespace MomVibe.Application.DTOs.Users;

using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.Constants;

public class UpdateProfileDto
{
    [MaxLength(ApplicationUserConstants.Lengths.DisplayNameMax)]
    public string? DisplayName { get; set; }

    [MaxLength(ApplicationUserConstants.Lengths.BioMax)]
    public string? Bio { get; set; }

    [MaxLength(ApplicationUserConstants.Lengths.AvatarUrlMax)]
    public string? AvatarUrl { get; set; }

    public ProfileType? ProfileType { get; set; }

    [MaxLength(ApplicationUserConstants.Lengths.LanguageCodeMax)]
    public string? LanguagePreference { get; set; }

    [MaxLength(ApplicationUserConstants.Lengths.RevolutTagMax)]
    public string? RevolutTag { get; set; }
}
