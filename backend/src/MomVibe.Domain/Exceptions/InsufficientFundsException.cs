namespace MomVibe.Domain.Exceptions;

/// <summary>
/// Thrown when a debit operation cannot complete because the wallet balance is too low.
/// The message is safe to surface in API error responses.
/// </summary>
public class InsufficientFundsException : DomainException
{
    public InsufficientFundsException()
        : base("Insufficient wallet balance to complete this transaction.") { }
}
