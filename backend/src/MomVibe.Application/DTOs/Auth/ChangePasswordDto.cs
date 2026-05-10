namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for changing user password.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>Gets or sets the user's current password for verification before the change is applied.</summary>
    public required string CurrentPassword { get; set; }

    /// <summary>Gets or sets the new password to replace the current one.</summary>
    public required string NewPassword { get; set; }

    /// <summary>Gets or sets the confirmation of the new password; must match <see cref="NewPassword"/>.</summary>
    public required string ConfirmNewPassword { get; set; }
}
