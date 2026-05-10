namespace MomVibe.Domain.Exceptions;

/// <summary>
/// Represents a user-facing business rule violation whose Message is safe to include
/// in API error responses. Use this instead of <see cref="System.InvalidOperationException"/>
/// when the error text is intended for end-users (e.g. "Email already registered.").
/// <see cref="System.InvalidOperationException"/> is treated as an internal error and
/// returns a generic message by the exception-handling middleware.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="DomainException"/> with the specified user-facing message.
    /// </summary>
    /// <param name="message">The error message that is safe to expose to end-users.</param>
    public DomainException(string message) : base(message) { }
}
