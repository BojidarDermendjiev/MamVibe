namespace MomVibe.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Configuration;

using Domain.Enums;
using Domain.Entities;
using Infrastructure.Services;
public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "ThisIsAVeryLongSecretKeyForTestingPurposes123456!",
                ["JwtSettings:Issuer"] = "MomVibe-Test",
                ["JwtSettings:Audience"] = "MomVibe-Test",
                ["JwtSettings:ExpirationMinutes"] = "60"
            })
            .Build();
        _tokenService = new TokenService(config);
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

    [Fact]
    public async Task GetPrincipalFromExpiredToken_Should_Return_Claims()
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

        var principal = _tokenService.GetPrincipalFromExpiredToken(token);

        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
    }
}
