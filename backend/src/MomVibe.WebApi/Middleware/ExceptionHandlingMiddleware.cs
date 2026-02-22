namespace MomVibe.WebApi.Middleware;

using System.Net;
using System.Text.Json;
using FluentValidation;
using MomVibe.Domain.Exceptions;


/// <summary>
/// Global exception handling middleware that:
/// - Catches unhandled exceptions in the pipeline
/// - Logs the error
/// - Maps common exception types to appropriate HTTP status codes
/// - Returns a JSON error response with camelCase properties
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/>.
    /// </summary>
    /// <param name="next">The next request delegate in the pipeline.</param>
    /// <param name="logger">Logger for recording unhandled exceptions.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this._next = next;
        this._logger = logger;
    }

    /// <summary>
    /// Invokes the middleware and intercepts unhandled exceptions.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this._next(context);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Converts an exception into a standardized JSON HTTP response.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest,
                string.Join(" ", ve.Errors.Select(e => e.ErrorMessage))),
            DomainException de => (HttpStatusCode.BadRequest, de.Message),      // Safe user-facing message
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Access denied."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Operation failed. Please try again."),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid request."),
            _ => (HttpStatusCode.InternalServerError, "An internal server error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new { error = message, statusCode = (int)statusCode };
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
