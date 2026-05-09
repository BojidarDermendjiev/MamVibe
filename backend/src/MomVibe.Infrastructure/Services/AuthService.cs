namespace MomVibe.Infrastructure.Services;

using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

using Domain.Entities;
using Domain.Exceptions;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.Interfaces;
using Infrastructure.Configuration;

/// <summary>
/// Authentication service for user registration, login, Google sign-in, JWT issuance and refresh,
/// refresh token persistence and revocation, and current user retrieval.
/// Integrates ASP.NET Core Identity, EF Core, AutoMapper, and configuration for secure, scalable auth flows.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IN8nWebhookService _webhook;
    private readonly N8nSettings _n8nSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IApplicationDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        IEmailService emailService,
        IN8nWebhookService webhook,
        IOptions<N8nSettings> n8nSettings)
    {
        this._userManager = userManager;
        this._tokenService = tokenService;
        this._context = context;
        this._mapper = mapper;
        this._configuration = configuration;
        this._emailService = emailService;
        this._webhook = webhook;
        this._n8nSettings = n8nSettings.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await this._userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new DomainException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            ProfileType = request.ProfileType,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await this._userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new DomainException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await this._userManager.AddToRoleAsync(user, "User");

        try
        {
            this._webhook.Send(this._n8nSettings.UserRegistered, new
            {
                Event = "user.registered",
                Timestamp = DateTime.UtcNow,
                Email = MaskEmail(user.Email),
                user.DisplayName,
                ProfileType = user.ProfileType.ToString(),
                user.LanguagePreference
            });
        }
        catch { /* Webhook failure must not break registration flow */ }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await this._userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.IsBlocked)
            throw new UnauthorizedAccessException("Your account has been blocked.");

        var isValid = await this._userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await this._context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == tokenHash);

        if (storedToken == null || !storedToken.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await this._userManager.FindByIdAsync(storedToken.UserId);
        if (user == null || user.IsBlocked)
            throw new UnauthorizedAccessException("User not found or blocked.");

        // Generate new tokens before revoking old one so we can record the replacement chain.
        var roles = await this._userManager.GetRolesAsync(user);
        var newAccessToken = await this._tokenService.GenerateAccessTokenAsync(user, roles);
        var newRefreshToken = this._tokenService.GenerateRefreshToken();
        var newTokenHash = HashToken(newRefreshToken);

        // Revoke the consumed token and record which token replaces it (rotation chain).
        // This allows detecting reuse: if an attacker uses a revoked token, the family can be revoked.
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = newTokenHash;

        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newTokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7"))
        };

        this._context.RefreshTokens.Add(newRefreshTokenEntity);
        await this._context.SaveChangesAsync();

        var userDto = this._mapper.Map<UserDto>(user);
        userDto.Roles = roles.ToList();

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15")),
            User = userDto
        };
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [this._configuration["GoogleAuth:ClientId"]!]
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

        var user = await this._userManager.FindByEmailAsync(payload.Email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                DisplayName = payload.Name ?? payload.Email.Split('@')[0],
                ProfileType = request.ProfileType,
                AvatarUrl = payload.Picture,
                EmailConfirmed = payload.EmailVerified,
                CreatedAt = DateTime.UtcNow
            };

            var result = await this._userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new DomainException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await this._userManager.AddToRoleAsync(user, "User");
        }

        if (user.IsBlocked)
            throw new UnauthorizedAccessException("Your account has been blocked.");

        // Refresh the Google avatar URL only when the stored URL is also from Google
        // (or absent). If the user has set a custom avatar we must not overwrite it.
        bool storedIsGoogle = string.IsNullOrEmpty(user.AvatarUrl)
            || user.AvatarUrl.StartsWith("https://lh3.googleusercontent.com", StringComparison.OrdinalIgnoreCase)
            || user.AvatarUrl.StartsWith("https://googleusercontent.com", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(payload.Picture) && storedIsGoogle && user.AvatarUrl != payload.Picture)
        {
            user.AvatarUrl = payload.Picture;
            await this._userManager.UpdateAsync(user);
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task RevokeTokenAsync(string userId)
    {
        var tokens = await this._context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await this._context.SaveChangesAsync();
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await this._userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var dto = this._mapper.Map<UserDto>(user);
        dto.Roles = (await this._userManager.GetRolesAsync(user)).ToList();
        return dto;
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new InvalidOperationException("New passwords do not match.");

        var user = await this._userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var result = await this._userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await this._userManager.FindByEmailAsync(email);
        if (user == null) return; // Don't reveal whether user exists

        var token = await this._userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var frontendUrl = this._configuration["FrontendUrl"] ?? "https://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

        var htmlBody = $@"
            <h2>Password Reset</h2>
            <p>You requested a password reset for your MomVibe account.</p>
            <p>Click the link below to reset your password:</p>
            <p><a href=""{resetLink}"">Reset Password</a></p>
            <p>If you didn't request this, please ignore this email.</p>";

        await this._emailService.SendEmailAsync(email, "MomVibe - Password Reset", htmlBody);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new InvalidOperationException("Passwords do not match.");

        var user = await this._userManager.FindByEmailAsync(dto.Email)
            ?? throw new InvalidOperationException("Invalid reset request.");

        var result = await this._userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    /// <summary>
    /// Returns a masked email for logging/webhook payloads to avoid exposing PII.
    /// e.g. "john.doe@example.com" → "jo***@example.com"
    /// </summary>
    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return "***";
        var at = email.IndexOf('@');
        if (at <= 0) return "***";
        var local = email[..at];
        var domain = email[at..];
        return (local.Length <= 2 ? "***" : local[..2] + "***") + domain;
    }

    /// <summary>
    /// Returns a lowercase hex-encoded SHA-256 hash of the given token.
    /// Used so plaintext refresh tokens are never stored in the database.
    /// </summary>
    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await this._userManager.GetRolesAsync(user);
        var accessToken = await this._tokenService.GenerateAccessTokenAsync(user, roles);
        var refreshToken = this._tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Token = HashToken(refreshToken),   // Store SHA-256 hash; plaintext goes to httpOnly cookie only
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7"))
        };

        this._context.RefreshTokens.Add(refreshTokenEntity);
        await this._context.SaveChangesAsync();

        var userDto = this._mapper.Map<UserDto>(user);
        userDto.Roles = roles.ToList();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15")),
            User = userDto
        };
    }
}
