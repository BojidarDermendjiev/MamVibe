using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

using MomVibe.Application.DTOs.Auth;
using MomVibe.Application.Interfaces;
using MomVibe.Application.Mapping;
using MomVibe.Domain.Entities;
using MomVibe.Domain.Enums;
using MomVibe.Domain.Exceptions;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Persistence;
using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for AuthService using an EF Core InMemory database.
/// UserManager and external dependencies (email, webhook, audit) are Moq mocks.
/// </summary>
public class AuthServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"AuthTest_{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        return cfg.CreateMapper();
    }

    private static IConfiguration CreateConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FrontendUrl"] = "https://localhost:5173"
            })
            .Build();

    private static IOptions<JwtSettings> CreateJwtOptions() =>
        Options.Create(new JwtSettings
        {
            Secret = "SuperSecretKeyForTestingPurposes123456!",
            Issuer = "MomVibe-Test",
            Audience = "MomVibe-Test",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    private static AuthService CreateService(
        ApplicationDbContext db,
        Mock<UserManager<ApplicationUser>> umMock,
        Mock<IEmailService>? emailMock = null,
        Mock<IAuditLogService>? auditMock = null)
    {
        emailMock ??= new Mock<IEmailService>();
        auditMock ??= new Mock<IAuditLogService>();

        var jwtOptions = CreateJwtOptions();
        var tokenService = new TokenService(jwtOptions);

        return new AuthService(
            umMock.Object,
            tokenService,
            db,
            CreateMapper(),
            CreateConfig(),
            emailMock.Object,
            jwtOptions,
            auditMock.Object,
            new Mock<MediatR.IPublisher>().Object,
            NullLogger<AuthService>.Instance);
    }

    // =========================================================================
    // RegisterAsync
    // =========================================================================

    [Fact]
    public async Task RegisterAsync_Success_Returns_AuthResponse_With_Tokens()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var capturedUser = (ApplicationUser?)null;

        umMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        umMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((u, _) => capturedUser = u)
            .ReturnsAsync(IdentityResult.Success);

        umMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        umMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        var svc = CreateService(db, umMock);
        var request = new RegisterRequestDto
        {
            Email = "newuser@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DisplayName = "New User",
            ProfileType = ProfileType.Female
        };

        var result = await svc.RegisterAsync(request);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        var storedToken = await db.RefreshTokens.FirstOrDefaultAsync();
        storedToken.Should().NotBeNull();
        storedToken!.UserId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_Throws_DomainException_When_Email_Already_Exists()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByEmailAsync("existing@test.com"))
            .ReturnsAsync(new ApplicationUser
            {
                Id = "existing-id",
                Email = "existing@test.com",
                DisplayName = "Existing",
                UserName = "existing@test.com"
            });

        var svc = CreateService(db, umMock);
        var request = new RegisterRequestDto
        {
            Email = "existing@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DisplayName = "Some User",
            ProfileType = ProfileType.Female
        };

        var act = async () => await svc.RegisterAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_Throws_DomainException_When_Identity_Creation_Fails()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        umMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var svc = CreateService(db, umMock);
        var request = new RegisterRequestDto
        {
            Email = "bad@test.com",
            Password = "weak",
            ConfirmPassword = "weak",
            DisplayName = "Bad User",
            ProfileType = ProfileType.Female
        };

        var act = async () => await svc.RegisterAsync(request);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Password too weak*");
    }

    // =========================================================================
    // LoginAsync
    // =========================================================================

    [Fact]
    public async Task LoginAsync_Success_Returns_AuthResponse_And_Logs_Audit()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var auditMock = new Mock<IAuditLogService>();
        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "login@test.com",
            DisplayName = "Login User",
            UserName = "login@test.com",
            IsBlocked = false
        };

        umMock.Setup(u => u.FindByEmailAsync("login@test.com")).ReturnsAsync(user);
        umMock.Setup(u => u.CheckPasswordAsync(user, "correct-pass")).ReturnsAsync(true);
        umMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var svc = CreateService(db, umMock, auditMock: auditMock);
        var result = await svc.LoginAsync(new LoginRequestDto
        {
            Email = "login@test.com",
            Password = "correct-pass"
        });

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("login@test.com");

        auditMock.Verify(a => a.LogAsync(user.Id, "Auth.Login", true,
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_Throws_Unauthorized_When_User_Not_Found()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var svc = CreateService(db, umMock);
        var act = async () => await svc.LoginAsync(new LoginRequestDto
        {
            Email = "ghost@test.com",
            Password = "any"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_Throws_Unauthorized_When_Password_Wrong()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "user@test.com",
            DisplayName = "User",
            UserName = "user@test.com",
            IsBlocked = false
        };

        umMock.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
        umMock.Setup(u => u.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

        var svc = CreateService(db, umMock);
        var act = async () => await svc.LoginAsync(new LoginRequestDto
        {
            Email = "user@test.com",
            Password = "wrong-pass"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_Throws_Unauthorized_When_Account_Blocked()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var user = new ApplicationUser
        {
            Id = "blocked-user",
            Email = "blocked@test.com",
            DisplayName = "Blocked",
            UserName = "blocked@test.com",
            IsBlocked = true
        };

        umMock.Setup(u => u.FindByEmailAsync("blocked@test.com")).ReturnsAsync(user);

        var svc = CreateService(db, umMock);
        var act = async () => await svc.LoginAsync(new LoginRequestDto
        {
            Email = "blocked@test.com",
            Password = "any"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*blocked*");
    }

    // =========================================================================
    // RefreshTokenAsync
    // =========================================================================

    [Fact]
    public async Task RefreshTokenAsync_Returns_New_Tokens_And_Rotates_Old_Token()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();

        // Store plaintext token; service will hash it
        var plaintext = "my-refresh-token-value";
        var hash = ComputeSha256Hex(plaintext);

        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "refresh@test.com",
            DisplayName = "Refresh User",
            UserName = "refresh@test.com",
            IsBlocked = false
        };

        db.RefreshTokens.Add(new RefreshToken
        {
            Token = hash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await db.SaveChangesAsync();

        umMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
        umMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var svc = CreateService(db, umMock);
        var result = await svc.RefreshTokenAsync(plaintext);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(plaintext);

        // Old token must now be revoked
        var oldToken = await db.RefreshTokens.FirstAsync(t => t.Token == hash);
        oldToken.RevokedAt.Should().NotBeNull();
        oldToken.ReplacedByToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshTokenAsync_Throws_Unauthorized_When_Token_Not_Found()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var svc = CreateService(db, umMock);

        var act = async () => await svc.RefreshTokenAsync("non-existent-token");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshTokenAsync_Throws_Unauthorized_When_Token_Already_Revoked()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();

        var plaintext = "revoked-token";
        var hash = ComputeSha256Hex(plaintext);

        db.RefreshTokens.Add(new RefreshToken
        {
            Token = hash,
            UserId = "user-1",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        });
        await db.SaveChangesAsync();

        var svc = CreateService(db, umMock);
        var act = async () => await svc.RefreshTokenAsync(plaintext);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    // =========================================================================
    // RevokeTokenAsync
    // =========================================================================

    [Fact]
    public async Task RevokeTokenAsync_Revokes_All_Active_Tokens_For_User()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();

        db.RefreshTokens.AddRange(
            new RefreshToken { Token = "hash-1", UserId = "user-1", ExpiresAt = DateTime.UtcNow.AddDays(7) },
            new RefreshToken { Token = "hash-2", UserId = "user-1", ExpiresAt = DateTime.UtcNow.AddDays(7) },
            new RefreshToken { Token = "hash-3", UserId = "user-2", ExpiresAt = DateTime.UtcNow.AddDays(7) }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db, umMock);
        await svc.RevokeTokenAsync("user-1");

        var user1Tokens = await db.RefreshTokens.Where(t => t.UserId == "user-1").ToListAsync();
        user1Tokens.Should().AllSatisfy(t => t.RevokedAt.Should().NotBeNull());

        var user2Token = await db.RefreshTokens.FirstAsync(t => t.UserId == "user-2");
        user2Token.RevokedAt.Should().BeNull("other users' tokens must not be affected");
    }

    // =========================================================================
    // ChangePasswordAsync
    // =========================================================================

    [Fact]
    public async Task ChangePasswordAsync_Throws_InvalidOperation_When_Passwords_Do_Not_Match()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var svc = CreateService(db, umMock);

        var act = async () => await svc.ChangePasswordAsync("user-1", new ChangePasswordDto
        {
            CurrentPassword = "Old123!",
            NewPassword = "New123!",
            ConfirmNewPassword = "Different123!"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("New passwords do not match.");
    }

    [Fact]
    public async Task ChangePasswordAsync_Throws_InvalidOperation_When_User_Not_Found()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByIdAsync("unknown")).ReturnsAsync((ApplicationUser?)null);
        var svc = CreateService(db, umMock);

        var act = async () => await svc.ChangePasswordAsync("unknown", new ChangePasswordDto
        {
            CurrentPassword = "Old123!",
            NewPassword = "New123!",
            ConfirmNewPassword = "New123!"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    // =========================================================================
    // ForgotPasswordAsync
    // =========================================================================

    [Fact]
    public async Task ForgotPasswordAsync_Sends_Reset_Email_When_User_Exists()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var emailMock = new Mock<IEmailService>();
        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "forgot@test.com",
            DisplayName = "Forgot User",
            UserName = "forgot@test.com"
        };

        umMock.Setup(u => u.FindByEmailAsync("forgot@test.com")).ReturnsAsync(user);
        umMock.Setup(u => u.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token-123");

        var svc = CreateService(db, umMock, emailMock);
        await svc.ForgotPasswordAsync("forgot@test.com");

        emailMock.Verify(e => e.SendEmailAsync(
            "forgot@test.com",
            It.Is<string>(s => s.Contains("Password Reset")),
            It.Is<string>(body => body.Contains("reset-password"))),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_Does_Not_Send_Email_When_User_Does_Not_Exist()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var emailMock = new Mock<IEmailService>();

        umMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var svc = CreateService(db, umMock, emailMock);
        await svc.ForgotPasswordAsync("nonexistent@test.com");

        emailMock.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    // =========================================================================
    // ResetPasswordAsync
    // =========================================================================

    [Fact]
    public async Task ResetPasswordAsync_Throws_InvalidOperation_When_Passwords_Do_Not_Match()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var svc = CreateService(db, umMock);

        var act = async () => await svc.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "user@test.com",
            Token = "some-token",
            NewPassword = "NewPass123!",
            ConfirmNewPassword = "Different123!"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Passwords do not match.");
    }

    [Fact]
    public async Task ResetPasswordAsync_Throws_InvalidOperation_When_User_Not_Found()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        var svc = CreateService(db, umMock);

        var act = async () => await svc.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "ghost@test.com",
            Token = "token",
            NewPassword = "NewPass123!",
            ConfirmNewPassword = "NewPass123!"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid reset request.");
    }

    [Fact]
    public async Task ResetPasswordAsync_Revokes_All_Sessions_On_Success()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var user = new ApplicationUser
        {
            Id = "user-reset",
            Email = "reset@test.com",
            UserName = "reset@test.com",
            DisplayName = "Reset User"
        };

        umMock.Setup(u => u.FindByEmailAsync("reset@test.com")).ReturnsAsync(user);
        umMock.Setup(u => u.ResetPasswordAsync(user, "valid-token", "NewPass123!"))
              .ReturnsAsync(IdentityResult.Success);

        db.RefreshTokens.AddRange(
            new RefreshToken { Token = "session-1", UserId = "user-reset", ExpiresAt = DateTime.UtcNow.AddDays(7) },
            new RefreshToken { Token = "session-2", UserId = "user-reset", ExpiresAt = DateTime.UtcNow.AddDays(7) }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db, umMock);
        await svc.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "reset@test.com",
            Token = "valid-token",
            NewPassword = "NewPass123!",
            ConfirmNewPassword = "NewPass123!"
        });

        var tokens = await db.RefreshTokens.Where(t => t.UserId == "user-reset").ToListAsync();
        tokens.Should().AllSatisfy(t => t.RevokedAt.Should().NotBeNull(),
            "all sessions must be invalidated after a password reset");
    }

    [Fact]
    public async Task ForgotPasswordAsync_Email_Contains_Security_Alert_And_Expiry_Notice()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var emailMock = new Mock<IEmailService>();
        var user = new ApplicationUser
        {
            Id = "user-alert",
            Email = "alert@test.com",
            UserName = "alert@test.com",
            DisplayName = "Alert User"
        };

        umMock.Setup(u => u.FindByEmailAsync("alert@test.com")).ReturnsAsync(user);
        umMock.Setup(u => u.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("alert-token");

        var svc = CreateService(db, umMock, emailMock);
        await svc.ForgotPasswordAsync("alert@test.com");

        emailMock.Verify(e => e.SendEmailAsync(
            "alert@test.com",
            It.IsAny<string>(),
            It.Is<string>(body =>
                body.Contains("30 minutes") &&
                body.Contains("did not request"))),
            Times.Once);
    }

    // =========================================================================
    // GetCurrentUserAsync
    // =========================================================================

    [Fact]
    public async Task GetCurrentUserAsync_Returns_Null_When_User_Not_Found()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByIdAsync("unknown")).ReturnsAsync((ApplicationUser?)null);

        var svc = CreateService(db, umMock);
        var result = await svc.GetCurrentUserAsync("unknown");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_Returns_UserDto_With_Roles()
    {
        await using var db = CreateDb();
        var umMock = CreateUserManagerMock();
        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "me@test.com",
            DisplayName = "Me",
            UserName = "me@test.com"
        };

        umMock.Setup(u => u.FindByIdAsync("user-1")).ReturnsAsync(user);
        umMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User", "Admin" });

        var svc = CreateService(db, umMock);
        var result = await svc.GetCurrentUserAsync("user-1");

        result.Should().NotBeNull();
        result!.Email.Should().Be("me@test.com");
        result.Roles.Should().Contain("Admin");
    }

    // =========================================================================
    // Private helper — mirrors the HashToken method inside AuthService
    // =========================================================================

    private static string ComputeSha256Hex(string input)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
