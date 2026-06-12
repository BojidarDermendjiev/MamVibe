namespace MomVibe.Domain.Exceptions;

/// <summary>
/// Thrown when an anti-abuse device-fingerprint check blocks a request — most
/// commonly the "this device already has an active business profile" rule.
/// Maps to HTTP 403 with the <see cref="Code"/> surfaced in the response body.
/// </summary>
public class DeviceConflictException : DomainException
{
    /// <summary>Stable machine-readable code (snake_case) for client UX.</summary>
    public string Code { get; }

    public DeviceConflictException(string code, string message) : base(message)
    {
        Code = code;
    }
}
