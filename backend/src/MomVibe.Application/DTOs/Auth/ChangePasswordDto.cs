namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for changing user password.
/// </summary>
public class ChangePasswordDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmNewPassword { get; set; }
}
