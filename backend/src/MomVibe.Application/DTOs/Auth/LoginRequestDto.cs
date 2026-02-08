namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Request payload for standard email/password authentication:
/// includes required Email and Password fields.
/// </summary>
public class LoginRequestDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
