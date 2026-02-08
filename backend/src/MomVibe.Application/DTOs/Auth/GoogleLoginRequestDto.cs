namespace MomVibe.Application.DTOs.Auth;

using Domain.Enums;

/// <summary>
/// Request payload for Google sign-in:
/// - IdToken: Google OpenID Connect ID token obtained on the client.
/// - ProfileType: desired user profile type; defaults to Family.
/// </summary>
public class GoogleLoginRequestDto
{
    public required string IdToken { get; set; }
    public ProfileType ProfileType { get; set; } = ProfileType.Family;
}
