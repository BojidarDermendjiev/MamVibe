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

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        this._httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => this._httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public bool IsAuthenticated => this._httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    public bool IsAdmin => this._httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
}
