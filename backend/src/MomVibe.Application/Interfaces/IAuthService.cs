namespace MomVibe.Application.Interfaces;

using DTOs.Auth;
using DTOs.Users;

/// <summary>
/// Authentication service contract for user registration, login (including Google sign-in),
/// JWT refresh, refresh token revocation, and retrieval of the current user as a DTO.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);
    Task RevokeTokenAsync(string userId);
    Task<UserDto?> GetCurrentUserAsync(string userId);
    Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
