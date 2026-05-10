namespace MomVibe.Application.DTOs.Auth;

using Domain.Enums;

/// <summary>
/// Request payload for user registration:
/// - Email: required.
/// - Password: required.
/// - ConfirmPassword: must match Password.
/// - DisplayName: required.
/// - ProfileType: selected user profile type.
/// Validation (via RegisterRequestValidator) enforces email format and password strength.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>Gets or sets the email address for the new account.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the desired password for the new account.</summary>
    public required string Password { get; set; }

    /// <summary>Gets or sets the confirmation of the desired password; must match <see cref="Password"/>.</summary>
    public required string ConfirmPassword { get; set; }

    /// <summary>Gets or sets the public display name for the new user profile.</summary>
    public required string DisplayName { get; set; }

    /// <summary>Gets or sets the profile type selected by the user during registration.</summary>
    public ProfileType ProfileType { get; set; }
}
