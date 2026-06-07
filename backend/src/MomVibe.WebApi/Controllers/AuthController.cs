namespace MomVibe.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;

using FluentValidation;

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
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ITurnstileService _turnstileService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        IWebHostEnvironment env,
        IConfiguration configuration,
        ITurnstileService turnstileService,
        IValidator<RegisterRequestDto> registerValidator)
    {
        this._authService = authService;
        this._currentUserService = currentUserService;
        this._env = env;
        this._configuration = configuration;
        this._turnstileService = turnstileService;
        this._registerValidator = registerValidator;
    }

    private string? ClientIp =>
        this.HttpContext.Connection.RemoteIpAddress?.ToString();

    private async Task<bool> VerifyTurnstileAsync(string? token)
    {
        if (this._env.IsDevelopment()) return true;
        if (string.IsNullOrWhiteSpace(token)) return false;
        var ip = this.ClientIp ?? string.Empty;
        return string.IsNullOrEmpty(ip)
            ? await this._turnstileService.VerifyTokenAsync(token)
            : await this._turnstileService.VerifyAsync(token, ip);
    }

    // Simple body model for mobile clients that cannot use httpOnly cookies.
    public sealed class MobileRefreshBody
    {
        public string? RefreshToken { get; set; }
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
            Path = "/api/v1/auth"
        });
    }

    private void ClearRefreshTokenCookie() =>
        this.Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure   = !this._env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path     = "/api/v1/auth"
        });

    /// <summary>
    /// Shapes the JSON response body for auth flows. Mobile clients (signalled by the
    /// <c>X-Client: mobile</c> header) receive the refresh token in the body because they
    /// cannot rely on httpOnly cookies; web clients receive only the access token in the
    /// body, with the refresh token delivered via the httpOnly cookie set by
    /// <see cref="SetRefreshTokenCookie"/>.
    /// </summary>
    private object BuildAuthResponseBody(Application.DTOs.Auth.AuthResponseDto result)
    {
        var isMobile = this.Request.Headers["X-Client"].FirstOrDefault()?.Equals("mobile", StringComparison.OrdinalIgnoreCase) == true;
        return isMobile
            ? new { accessToken = result.AccessToken, refreshToken = result.RefreshToken, user = result.User, expiresAt = result.ExpiresAt }
            : (object)new { accessToken = result.AccessToken, user = result.User, expiresAt = result.ExpiresAt };
    }

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
        if (!await this.VerifyTurnstileAsync(request.TurnstileToken))
            return BadRequest(new { message = "Bot verification failed. Please try again." });

        var validation = await this._registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        try
        {
            var result = await this._authService.RegisterAsync(request);
            this.SetRefreshTokenCookie(result.RefreshToken);
            return Ok(this.BuildAuthResponseBody(result));
        }
        catch (InvalidOperationException ex)
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
        if (!await this.VerifyTurnstileAsync(request.TurnstileToken))
            return BadRequest(new { message = "Bot verification failed. Please try again." });

        try
        {
            var result = await this._authService.LoginAsync(request);
            this.SetRefreshTokenCookie(result.RefreshToken);
            return Ok(this.BuildAuthResponseBody(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
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
    public async Task<IActionResult> RefreshToken(
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] MobileRefreshBody? body = null)
    {
        // Cookie takes priority (web); fall back to body for native mobile clients.
        var refreshToken = this.Request.Cookies["refreshToken"] ?? body?.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "No refresh token." });

        try
        {
            var result = await this._authService.RefreshTokenAsync(refreshToken);
            this.SetRefreshTokenCookie(result.RefreshToken);
            return Ok(this.BuildAuthResponseBody(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            this.ClearRefreshTokenCookie();
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
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
            return Ok(this.BuildAuthResponseBody(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
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
    /// Changes the password for the currently authenticated user.
    /// </summary>
    /// <param name="dto">Payload containing the current password and the desired new password.</param>
    /// <returns>
    /// 401 Unauthorized if the current user context is missing.<br/>
    /// 400 Bad Request with an error message if the current password is incorrect or the new password fails validation.<br/>
    /// 200 OK with a success message on completion.
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Initiates the password reset flow by sending a reset link to the user's email address.
    /// Errors are suppressed to prevent email enumeration attacks.
    /// </summary>
    /// <param name="dto">Payload containing the email address for which to send the reset link.</param>
    /// <returns>
    /// 200 OK with a confirmation message regardless of whether the email exists.
    /// </returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (!await this.VerifyTurnstileAsync(dto.TurnstileToken))
            return BadRequest(new { message = "Bot verification failed. Please try again." });

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

    /// <summary>
    /// Resets a user's password using a token issued by the forgot-password flow.
    /// </summary>
    /// <param name="dto">Payload containing the reset token, email, and new password.</param>
    /// <returns>
    /// 400 Bad Request with an error message if the token is invalid or expired.<br/>
    /// 200 OK with a success message on completion.
    /// </returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            await this._authService.ResetPasswordAsync(dto);
            return Ok(new { message = "Password reset successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
