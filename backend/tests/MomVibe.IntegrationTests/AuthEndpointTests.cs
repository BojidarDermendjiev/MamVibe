using System.Net;
using FluentAssertions;
using System.Net.Http.Json;

using MomVibe.Domain.Enums;
using MomVibe.Application.DTOs.Auth;
using MomVibe.Application.DTOs.Users;

namespace MomVibe.IntegrationTests;

public class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    // Matches the non-mobile JSON body shape: refreshToken is in HttpOnly cookie, not the body.
    private record WebAuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        var request = new RegisterRequestDto
        {
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DisplayName = "Test User",
            ProfileType = ProfileType.Female
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<WebAuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        var email = $"dup_{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequestDto
        {
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DisplayName = "Test User",
            ProfileType = ProfileType.Male
        };

        await _client.PostAsJsonAsync("/api/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        var email = $"login_{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DisplayName = "Login Test",
            ProfileType = ProfileType.Family
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = email,
            Password = "Password123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<WebAuthResponse>();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var request = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Me_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithAuth_ShouldReturnUser()
    {
        var email = $"me_{Guid.NewGuid():N}@example.com";
        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            DisplayName = "Me Test",
            ProfileType = ProfileType.Female
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResult = await registerResponse.Content.ReadFromJsonAsync<WebAuthResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.AccessToken);

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
