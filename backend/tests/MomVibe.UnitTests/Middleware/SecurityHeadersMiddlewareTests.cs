using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

using MomVibe.WebApi.Middleware;

namespace MomVibe.UnitTests.Middleware;

/// <summary>
/// Unit tests for SecurityHeadersMiddleware.
/// Verifies that all OWASP-recommended headers are added to every response
/// and that the next middleware delegate is always called.
/// </summary>
public class SecurityHeadersMiddlewareTests
{
    private static SecurityHeadersMiddleware CreateMiddleware(RequestDelegate next) =>
        new SecurityHeadersMiddleware(next);

    // =========================================================================
    // Next delegate always called
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Always_Calls_Next()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        nextMock.Verify(n => n(context), Times.Once);
    }

    // =========================================================================
    // Individual OWASP headers
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Sets_X_Content_Type_Options_Header()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Content-Type-Options"].ToString()
            .Should().Be("nosniff");
    }

    [Fact]
    public async Task InvokeAsync_Sets_X_Frame_Options_Header()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Frame-Options"].ToString()
            .Should().Be("DENY");
    }

    [Fact]
    public async Task InvokeAsync_Sets_Referrer_Policy_Header()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["Referrer-Policy"].ToString()
            .Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task InvokeAsync_Sets_Content_Security_Policy_Header()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("object-src 'none'");
        csp.Should().Contain("upgrade-insecure-requests");
    }

    [Fact]
    public async Task InvokeAsync_Sets_Cross_Origin_Opener_Policy_Header()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Headers["Cross-Origin-Opener-Policy"].ToString()
            .Should().Be("same-origin-allow-popups");
    }

    [Fact]
    public async Task InvokeAsync_Sets_Permissions_Policy_Header()
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        var policy = context.Response.Headers["Permissions-Policy"].ToString();
        policy.Should().Contain("camera=()");
        policy.Should().Contain("microphone=()");
        policy.Should().Contain("geolocation=()");
    }
}
