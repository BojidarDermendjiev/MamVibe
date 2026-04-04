namespace MomVibe.Domain.Exceptions;

/// <summary>
/// Thrown when an operation is attempted on a wallet that is not in an <c>Active</c> state.
/// The message is safe to surface in API error responses.
/// </summary>
public class WalletFrozenException : DomainException
{
    public WalletFrozenException()
        : base("This wallet is currently frozen or suspended and cannot process transactions.") { }
}
