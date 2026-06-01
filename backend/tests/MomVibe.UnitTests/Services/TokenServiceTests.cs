using FluentAssertions;
using Microsoft.Extensions.Options;

using MomVibe.Domain.Enums;
using MomVibe.Domain.Entities;
using MomVibe.Infrastructure.Configuration;
using MomVibe.Infrastructure.Services;


namespace MomVibe.UnitTests.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var jwt = Options.Create(new JwtSettings
        {
            Secret = "ThisIsAVeryLongSecretKeyForTestingPurposes123456!",
            Issuer = "MomVibe-Test",
            Audience = "MomVibe-Test",
            AccessTokenExpirationMinutes = 60
        });
        _tokenService = new TokenService(jwt);
    }

    [Fact]
    public async Task GenerateAccessToken_Should_Return_Valid_Token()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            DisplayName = "Test User",
            ProfileType = ProfileType.Female
        };
        var roles = new List<string> { "User" };

        var token = await _tokenService.GenerateAccessTokenAsync(user, roles);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_Should_Return_Non_Empty_String()
    {
        var token = _tokenService.GenerateRefreshToken();

        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void GenerateRefreshToken_Should_Be_Unique()
    {
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        token1.Should().NotBe(token2);
    }

}
