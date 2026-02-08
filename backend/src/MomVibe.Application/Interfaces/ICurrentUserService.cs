namespace MomVibe.Application.Interfaces;

/// <summary>
/// Abstraction for accessing current request user information:
/// provides the user's ID, authentication status, and Admin role check.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
