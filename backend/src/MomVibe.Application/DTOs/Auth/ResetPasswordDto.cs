namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for resetting a forgotten password.
/// </summary>
public class ResetPasswordDto
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmNewPassword { get; set; }
}
