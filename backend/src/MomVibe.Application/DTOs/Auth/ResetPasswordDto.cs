namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for resetting a forgotten password.
/// </summary>
public class ResetPasswordDto
{
    /// <summary>Gets or sets the email address of the account whose password is being reset.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the password reset token received via email.</summary>
    public required string Token { get; set; }

    /// <summary>Gets or sets the new password to set on the account.</summary>
    public required string NewPassword { get; set; }

    /// <summary>Gets or sets the confirmation of the new password; must match <see cref="NewPassword"/>.</summary>
    public required string ConfirmNewPassword { get; set; }
}
