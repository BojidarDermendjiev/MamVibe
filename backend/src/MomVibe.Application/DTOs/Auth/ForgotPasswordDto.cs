namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for initiating a password reset request.
/// </summary>
public class ForgotPasswordDto
{
    public required string Email { get; set; }
}
