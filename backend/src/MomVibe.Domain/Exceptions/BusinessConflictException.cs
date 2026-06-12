namespace MomVibe.Domain.Exceptions;

/// <summary>
/// A business-vertical rule violation that warrants HTTP 409 Conflict rather than 400.
/// Carries a stable <see cref="Code"/> that clients can localize against (e.g.,
/// <c>profile_already_exists</c>, <c>policy_outdated</c>).
/// </summary>
public class BusinessConflictException : DomainException
{
    /// <summary>Stable machine-readable code (snake_case) for client localization.</summary>
    public string Code { get; }

    public BusinessConflictException(string code, string message) : base(message)
    {
        Code = code;
    }
}
