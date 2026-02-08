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
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public required string DisplayName { get; set; }
    public ProfileType ProfileType { get; set; }
}
