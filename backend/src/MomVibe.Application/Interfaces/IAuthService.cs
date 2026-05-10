namespace MomVibe.Application.Interfaces;

using DTOs.Auth;
using DTOs.Users;

/// <summary>
/// Authentication service contract for user registration, login (including Google sign-in),
/// JWT refresh, refresh token revocation, and retrieval of the current user as a DTO.
/// </summary>
public interface IAuthService
{
    /// <summary>Registers a new user account and returns an authenticated response.</summary>
    /// <param name="request">The registration payload containing email, password, display name, and profile type.</param>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>Authenticates a user with email and password and returns a token pair.</summary>
    /// <param name="request">The login credentials.</param>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>Exchanges a valid refresh token for a new access/refresh token pair.</summary>
    /// <param name="refreshToken">The refresh token to validate and exchange.</param>
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);

    /// <summary>Authenticates or registers a user via a Google OpenID Connect ID token.</summary>
    /// <param name="request">The Google sign-in request containing the ID token.</param>
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);

    /// <summary>Revokes all active refresh tokens for the specified user.</summary>
    /// <param name="userId">The identifier of the user whose tokens should be revoked.</param>
    Task RevokeTokenAsync(string userId);

    /// <summary>Returns the profile DTO for the specified user, or <c>null</c> if not found.</summary>
    /// <param name="userId">The identifier of the user to retrieve.</param>
    Task<UserDto?> GetCurrentUserAsync(string userId);

    /// <summary>Changes the password for the specified user after verifying the current password.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="dto">The current and new password values.</param>
    Task ChangePasswordAsync(string userId, ChangePasswordDto dto);

    /// <summary>Sends a password-reset email to the specified address if the account exists.</summary>
    /// <param name="email">The email address associated with the account.</param>
    Task ForgotPasswordAsync(string email);

    /// <summary>Resets the password using the token received via email.</summary>
    /// <param name="dto">The reset payload containing email, token, and new password.</param>
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
