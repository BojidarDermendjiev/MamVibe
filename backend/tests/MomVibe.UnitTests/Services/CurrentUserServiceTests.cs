using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

using MomVibe.Infrastructure.Services;

namespace MomVibe.UnitTests.Services;

/// <summary>
/// Unit tests for CurrentUserService which wraps IHttpContextAccessor.
/// No database is involved — the tests only set up ClaimsPrincipal instances.
/// </summary>
public class CurrentUserServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static CurrentUserService CreateServiceWithClaims(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        return new CurrentUserService(accessor.Object);
    }

    private static CurrentUserService CreateServiceWithNoContext()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        return new CurrentUserService(accessor.Object);
    }

    private static CurrentUserService CreateServiceWithUnauthenticatedUser()
    {
        var identity = new ClaimsIdentity(); // no authentication type → IsAuthenticated = false
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        return new CurrentUserService(accessor.Object);
    }

    // =========================================================================
    // UserId
    // =========================================================================

    [Fact]
    public void UserId_Returns_NameIdentifier_Claim_Value_When_Authenticated()
    {
        var svc = CreateServiceWithClaims(
            new Claim(ClaimTypes.NameIdentifier, "user-abc-123"));

        svc.UserId.Should().Be("user-abc-123");
    }

    [Fact]
    public void UserId_Returns_Null_When_HttpContext_Is_Null()
    {
        var svc = CreateServiceWithNoContext();

        svc.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_Returns_Null_When_No_NameIdentifier_Claim_Present()
    {
        var svc = CreateServiceWithClaims(
            new Claim(ClaimTypes.Email, "user@test.com")); // email only, no NameIdentifier

        svc.UserId.Should().BeNull();
    }

    // =========================================================================
    // IsAuthenticated
    // =========================================================================

    [Fact]
    public void IsAuthenticated_Returns_True_When_ClaimsIdentity_Has_AuthenticationType()
    {
        var svc = CreateServiceWithClaims(
            new Claim(ClaimTypes.NameIdentifier, "user-1"));

        svc.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_Returns_False_When_HttpContext_Is_Null()
    {
        var svc = CreateServiceWithNoContext();

        svc.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_Returns_False_When_Identity_Has_No_AuthenticationType()
    {
        var svc = CreateServiceWithUnauthenticatedUser();

        svc.IsAuthenticated.Should().BeFalse();
    }

    // =========================================================================
    // IsAdmin
    // =========================================================================

    [Fact]
    public void IsAdmin_Returns_True_When_User_Has_Admin_Role_Claim()
    {
        var svc = CreateServiceWithClaims(
            new Claim(ClaimTypes.NameIdentifier, "admin-1"),
            new Claim(ClaimTypes.Role, "Admin"));

        svc.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_Returns_False_When_User_Lacks_Admin_Role_Claim()
    {
        var svc = CreateServiceWithClaims(
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Role, "User"));

        svc.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_Returns_False_When_HttpContext_Is_Null()
    {
        var svc = CreateServiceWithNoContext();

        svc.IsAdmin.Should().BeFalse();
    }
}
