using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

using MomVibe.Domain.Exceptions;
using MomVibe.WebApi.Middleware;

namespace MomVibe.UnitTests.Middleware;

/// <summary>
/// Unit tests for ExceptionHandlingMiddleware.
/// Uses DefaultHttpContext and a ResponseBodySpy to capture the JSON response.
/// Verifies that each exception type maps to the correct HTTP status code and JSON message.
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

    private static ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next) =>
        new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

    private static (HttpContext Context, MemoryStream Body) CreateContext()
    {
        var context = new DefaultHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;
        return (context, body);
    }

    private static async Task<(int StatusCode, string Json)> InvokeAndReadAsync(
        ExceptionHandlingMiddleware middleware, HttpContext context, MemoryStream body)
    {
        await middleware.InvokeAsync(context);
        body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(body).ReadToEndAsync();
        return (context.Response.StatusCode, json);
    }

    // =========================================================================
    // Happy path — next middleware runs without exception
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Calls_Next_When_No_Exception()
    {
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = CreateMiddleware(nextMock.Object);
        var (context, _) = CreateContext();

        await middleware.InvokeAsync(context);

        nextMock.Verify(n => n(context), Times.Once);
        context.Response.StatusCode.Should().Be(200, "default status should be unchanged when no exception");
    }

    // =========================================================================
    // ValidationException (FluentValidation) → 400
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_400_For_ValidationException()
    {
        var failures = new[] { new ValidationFailure("Email", "Email is required") };
        var exception = new ValidationException(failures);

        var next = new RequestDelegate(_ => throw exception);
        var middleware = CreateMiddleware(next);
        var (context, body) = CreateContext();

        var (statusCode, json) = await InvokeAndReadAsync(middleware, context, body);

        statusCode.Should().Be((int)HttpStatusCode.BadRequest);
        json.Should().Contain("Email is required");
        context.Response.ContentType.Should().Be("application/json");
    }

    // =========================================================================
    // DomainException → 400
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_400_For_DomainException_With_Safe_Message()
    {
        var exception = new DomainException("Email already registered.");
        var next = new RequestDelegate(_ => throw exception);
        var middleware = CreateMiddleware(next);
        var (context, body) = CreateContext();

        var (statusCode, json) = await InvokeAndReadAsync(middleware, context, body);

        statusCode.Should().Be((int)HttpStatusCode.BadRequest);
        json.Should().Contain("Email already registered.");
    }

    // =========================================================================
    // UnauthorizedAccessException → 401
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_401_For_UnauthorizedAccessException()
    {
        var exception = new UnauthorizedAccessException("No access");
        var next = new RequestDelegate(_ => throw exception);
        var middleware = CreateMiddleware(next);
        var (context, body) = CreateContext();

        var (statusCode, json) = await InvokeAndReadAsync(middleware, context, body);

        statusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        json.Should().Contain("Access denied");
    }

    // =========================================================================
    // KeyNotFoundException → 404
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_404_For_KeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Item not found");
        var next = new RequestDelegate(_ => throw exception);
        var middleware = CreateMiddleware(next);
        var (context, body) = CreateContext();

        var (statusCode, json) = await InvokeAndReadAsync(middleware, context, body);

        statusCode.Should().Be((int)HttpStatusCode.NotFound);
        json.Should().Contain("not found");
    }

    // =========================================================================
    // InvalidOperationException → 400
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_400_For_InvalidOperationException()
    {
        var exception = new InvalidOperationException("Cannot do that");
        var next = new RequestDelegate(_ => throw exception);
        var middleware = CreateMiddleware(next);
        var (context, body) = CreateContext();

        var (statusCode, json) = await InvokeAndReadAsync(middleware, context, body);

        statusCode.Should().Be((int)HttpStatusCode.BadRequest);
        json.Should().Contain("Cannot do that");
    }

    // =========================================================================
    // Generic Exception → 500
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Returns_500_For_Unhandled_Exception()
    {
        var exception = new Exception("Something exploded");
        var next = new RequestDelegate(_ => throw exception);
        var middleware = CreateMiddleware(next);
        var (context, body) = CreateContext();

        var (statusCode, json) = await InvokeAndReadAsync(middleware, context, body);

        statusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        json.Should().Contain("internal server error");
    }

    // =========================================================================
    // JSON response shape
    // =========================================================================

    [Fact]
    public async Task InvokeAsync_Response_Contains_Error_And_StatusCode_Properties()
    {
        var exception = new KeyNotFoundException("Missing");
        var next = new RequestDelegate(_ => throw exception);
        var middleware = CreateMiddleware(next);
        var (context, body) = CreateContext();

        await middleware.InvokeAsync(context);
        body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(body).ReadToEndAsync();

        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("error", out _).Should().BeTrue("response should have 'error' property");
        doc.RootElement.TryGetProperty("statusCode", out var statusCodeProp).Should().BeTrue("response should have 'statusCode' property");
        statusCodeProp.GetInt32().Should().Be(404);
    }
}
