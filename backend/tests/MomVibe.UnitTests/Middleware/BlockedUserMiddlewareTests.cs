using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

using MomVibe.Domain.Entities;
using MomVibe.WebApi.Middleware;

namespace MomVibe.UnitTests.Middleware;

/// <summary>
/// Unit tests for BlockedUserMiddleware.
/// Uses DefaultHttpContext with a ClaimsPrincipal to simulate authenticated requests.
/// IDistributedCache and UserManager are Moq mocks.
/// </summary>
public class BlockedUserMiddlewareTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static BlockedUserMiddleware CreateMiddleware(RequestDelegate next) =>
        new BlockedUserMiddleware(next);

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    /// <summary>Creates an authenticated context for the given userId.</summary>
    private static DefaultHttpContext CreateAuthenticatedContext(string userId)
    {
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    // =========================================================================
    // Unauthenticated requests — should pass through
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Passes_Through_For_Unauthenticated_Request()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = new DefaultHttpContext(); // Not authenticated
        var umMock = CreateUserManagerMock();
        var cacheMock = new Mock<IDistributedCache>();

        await middleware.InvokeAsync(context, umMock.Object, cacheMock.Object);

        nextMock.Verify(n => n(context), Times.Once);
        context.Response.StatusCode.Should().Be(200);
    }

    // =========================================================================
    // Blocked user (from UserManager) → 403
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_403_When_User_Is_Blocked()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "blocked-user") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByIdAsync("blocked-user"))
            .ReturnsAsync(new ApplicationUser { Id = "blocked-user", IsBlocked = true, DisplayName = "Blocked", UserName = "blocked", Email = "b@test.com" });

        // Cache returns null (cache miss → goes to UserManager)
        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((byte[]?)null);

        await middleware.InvokeAsync(context, umMock.Object, cacheMock.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    // =========================================================================
    // Non-blocked user (from UserManager) → passes through
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Passes_Through_When_User_Is_Not_Blocked()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = CreateAuthenticatedContext("active-user");

        var umMock = CreateUserManagerMock();
        umMock.Setup(u => u.FindByIdAsync("active-user"))
            .ReturnsAsync(new ApplicationUser { Id = "active-user", IsBlocked = false, DisplayName = "Active", UserName = "active", Email = "a@test.com" });

        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((byte[]?)null);

        await middleware.InvokeAsync(context, umMock.Object, cacheMock.Object);

        nextMock.Verify(n => n(context), Times.Once);
        context.Response.StatusCode.Should().Be(200);
    }

    // =========================================================================
    // Blocked status served from cache → 403 without hitting UserManager
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_403_From_Cache_Without_Hitting_UserManager()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "cached-blocked") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var umMock = CreateUserManagerMock();

        // Cache says blocked (byte value 1)
        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.GetAsync("blocked:cached-blocked", default))
            .ReturnsAsync(new byte[] { 1 });

        await middleware.InvokeAsync(context, umMock.Object, cacheMock.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        // UserManager should NOT be called since result came from cache
        umMock.Verify(u => u.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    // =========================================================================
    // Non-blocked status served from cache → passes through without hitting UserManager
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Passes_Through_From_Cache_Without_Hitting_UserManager()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = CreateAuthenticatedContext("cached-active");

        var umMock = CreateUserManagerMock();

        // Cache says not blocked (byte value 0)
        var cacheMock = new Mock<IDistributedCache>();
        cacheMock.Setup(c => c.GetAsync("blocked:cached-active", default))
            .ReturnsAsync(new byte[] { 0 });

        await middleware.InvokeAsync(context, umMock.Object, cacheMock.Object);

        nextMock.Verify(n => n(context), Times.Once);
        umMock.Verify(u => u.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    // =========================================================================
    // Cache key format
    // =========================================================================

    [Fact]
    public void CacheKey_Returns_Correct_Key_Format()
    {
        BlockedUserMiddleware.CacheKey("user-123").Should().Be("blocked:user-123");
    }
}
