namespace MomVibe.Infrastructure.Services;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

using Application.Interfaces;

/// <summary>
/// Service for accessing details about the current HTTP user from the request context.
/// Provides the user's unique identifier, authentication status, and whether they are in the Admin role.
/// Wraps IHttpContextAccessor to safely read claims from the current request.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initializes a new instance of <see cref="CurrentUserService"/> with the HTTP context accessor.</summary>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        this._httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Gets the current user's unique identifier from the NameIdentifier claim; null if unauthenticated.</summary>
    public string? UserId => this._httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    /// <summary>Gets a value indicating whether the current request has an authenticated user.</summary>
    public bool IsAuthenticated => this._httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    /// <summary>Gets a value indicating whether the current user is in the Admin role.</summary>
    public bool IsAdmin => this._httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
}
