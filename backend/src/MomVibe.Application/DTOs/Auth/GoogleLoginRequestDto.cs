namespace MomVibe.Application.DTOs.Auth;

using Domain.Enums;

/// <summary>
/// Request payload for Google sign-in:
/// - IdToken: Google OpenID Connect ID token obtained on the client.
/// - ProfileType: desired user profile type; defaults to Family.
/// </summary>
public class GoogleLoginRequestDto
{
    /// <summary>Gets or sets the Google OpenID Connect ID token obtained by the client after Google sign-in.</summary>
    public required string IdToken { get; set; }

    /// <summary>Gets or sets the desired profile type for new accounts; defaults to <see cref="ProfileType.Family"/>.</summary>
    public ProfileType ProfileType { get; set; } = ProfileType.Family;
}
