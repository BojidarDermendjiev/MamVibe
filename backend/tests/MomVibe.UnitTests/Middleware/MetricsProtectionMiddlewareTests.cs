using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

using MomVibe.WebApi.Middleware;

namespace MomVibe.UnitTests.Middleware;

/// <summary>
/// Unit tests for MetricsProtectionMiddleware.
/// Verifies that /metrics is blocked for external IPs and allowed for
/// localhost and Docker private address ranges.
/// </summary>
public class MetricsProtectionMiddlewareTests
{
    private static MetricsProtectionMiddleware CreateMiddleware(RequestDelegate next) =>
        new MetricsProtectionMiddleware(next);

    private static DefaultHttpContext CreateContextWithIp(string? ipAddress, string path = "/metrics")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        if (ipAddress != null)
            context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        return context;
    }

    // =========================================================================
    // Non-metrics paths — always pass through regardless of IP
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Passes_Through_Non_Metrics_Path_For_Any_IP()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = CreateContextWithIp("1.2.3.4", path: "/api/items");

        await middleware.InvokeAsync(context);

        nextMock.Verify(n => n(context), Times.Once);
        context.Response.StatusCode.Should().Be(200);
    }

    // =========================================================================
    // /metrics from external IP → 404
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_404_For_Metrics_Request_From_External_IP()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = CreateContextWithIp("8.8.8.8"); // Public IP

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        nextMock.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
    }

    // =========================================================================
    // /metrics from localhost → allowed
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Allows_Metrics_Request_From_IPv4_Loopback()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = CreateContextWithIp("127.0.0.1");

        await middleware.InvokeAsync(context);

        nextMock.Verify(n => n(context), Times.Once);
    }

    // =========================================================================
    // /metrics from Docker internal networks → allowed
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Allows_Metrics_From_Docker_172_Network()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = CreateContextWithIp("172.20.0.2"); // Docker default bridge network

        await middleware.InvokeAsync(context);

        nextMock.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Allows_Metrics_From_10_0_Network()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = CreateContextWithIp("10.0.0.5");

        await middleware.InvokeAsync(context);

        nextMock.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Allows_Metrics_From_192_168_Network()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = CreateContextWithIp("192.168.1.100");

        await middleware.InvokeAsync(context);

        nextMock.Verify(n => n(context), Times.Once);
    }

    // =========================================================================
    // IPv4-mapped IPv6 loopback (::ffff:127.0.0.1) → allowed
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Allows_Metrics_From_IPv4_Mapped_IPv6_Loopback()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        var middleware = CreateMiddleware(nextMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Path = "/metrics";
        // Dual-stack socket delivers IPv4 as ::ffff:127.0.0.1
        context.Connection.RemoteIpAddress = IPAddress.Parse("::ffff:127.0.0.1");

        await middleware.InvokeAsync(context);

        nextMock.Verify(n => n(context), Times.Once);
    }
}
