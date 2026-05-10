namespace MomVibe.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for initiating a password reset request.
/// </summary>
public class ForgotPasswordDto
{
    /// <summary>Gets or sets the email address of the account for which a password reset is requested.</summary>
    public required string Email { get; set; }
}
