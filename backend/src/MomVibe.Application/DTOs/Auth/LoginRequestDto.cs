namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Request payload for standard email/password authentication:
/// includes required Email and Password fields.
/// </summary>
public class LoginRequestDto
{
    /// <summary>Gets or sets the registered email address of the user.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the user's password.</summary>
    public required string Password { get; set; }
}
