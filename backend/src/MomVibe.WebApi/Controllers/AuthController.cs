namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;

using Application.DTOs.Auth;
using Application.Interfaces;

/// <summary>
/// Authentication API controller providing endpoints for:
/// - User registration
/// - Email/password login
/// - Token refresh
/// - Google OAuth login
/// - Token revocation (authorized)
/// - Retrieving current user profile (authorized)
/// </summary>

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        IWebHostEnvironment env,
        IConfiguration configuration)
    {
        this._authService = authService;
        this._currentUserService = currentUserService;
        this._env = env;
        this._configuration = configuration;
    }

    private void SetRefreshTokenCookie(string token)
    {
        var days = int.Parse(this._configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
        this.Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !this._env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(days),
            Path = "/api/auth"
        });
    }

    private void ClearRefreshTokenCookie() =>
        this.Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details including email, password, and profile type.</param>
    /// <returns>
    /// 200 OK with the registration result on success.<br/>
    /// 400 Bad Request with an error message on validation or processing failure.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await this._authService.RegisterAsync(request);
            this.SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new { accessToken = result.AccessToken, user = result.User, expiresAt = result.ExpiresAt });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">Credentials containing email and password.</param>
    /// <returns>
    /// 200 OK with authentication tokens and user info on success.<br/>
    /// 400 Bad Request with an error message on invalid credentials or failure.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await this._authService.LoginAsync(request);
            this.SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new { accessToken = result.AccessToken, user = result.User, expiresAt = result.ExpiresAt });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Exchanges a refresh token for new JWT credentials.
    /// </summary>
    /// <param name="request">Payload containing the current access and refresh tokens.</param>
    /// <returns>
    /// 200 OK with refreshed tokens on success.<br/>
    /// 400 Bad Request with an error message on invalid or expired refresh token.
    /// </returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = this.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "No refresh token." });

        try
        {
            var result = await this._authService.RefreshTokenAsync(refreshToken);
            this.SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new { accessToken = result.AccessToken, user = result.User, expiresAt = result.ExpiresAt });
        }
        catch (Exception ex)
        {
            this.ClearRefreshTokenCookie();
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user via Google OAuth using an ID token.
    /// </summary>
    /// <param name="request">Google login payload containing the ID token and desired profile type.</param>
    /// <returns>
    /// 200 OK with authentication result on success.<br/>
    /// 400 Bad Request with an error message on token validation failure.
    /// </returns>
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
    {
        try
        {
            var result = await this._authService.GoogleLoginAsync(request);
            this.SetRefreshTokenCookie(result.RefreshToken);
            return Ok(new { accessToken = result.AccessToken, user = result.User, expiresAt = result.ExpiresAt });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Revokes the current user's refresh token, effectively logging out all sessions tied to it.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 204 No Content on successful revocation.
    /// </returns>
    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        await this._authService.RevokeTokenAsync(userId);
        this.ClearRefreshTokenCookie();
        return NoContent();
    }

    /// <summary>
    /// Retrieves the profile of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 404 Not Found if the user cannot be located.<br/>
    /// 200 OK with the user profile on success.
    /// </returns>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        try
        {
            await this._authService.ChangePasswordAsync(userId, dto);
            return Ok(new { message = "Password changed successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            await this._authService.ForgotPasswordAsync(dto.Email);
        }
        catch
        {
            // Swallow errors to prevent email enumeration
        }
        return Ok(new { message = "If an account with that email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            await this._authService.ResetPasswordAsync(dto);
            return Ok(new { message = "Password reset successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = this._currentUserService.UserId;
        if (userId == null) return Unauthorized();
        var user = await this._authService.GetCurrentUserAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }
}
