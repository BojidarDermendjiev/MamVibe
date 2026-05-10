namespace MomVibe.Application.Interfaces;

/// <summary>
/// Abstraction for accessing current request user information:
/// provides the user's ID, authentication status, and Admin role check.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Gets the identifier of the authenticated user, or <c>null</c> if unauthenticated.</summary>
    string? UserId { get; }

    /// <summary>Gets a value indicating whether the current request is authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Gets a value indicating whether the authenticated user belongs to the Admin role.</summary>
    bool IsAdmin { get; }
}
