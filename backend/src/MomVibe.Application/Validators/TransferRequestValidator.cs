namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Wallet;
using Domain.Constants;

public class TransferRequestValidator : AbstractValidator<TransferRequestDto>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.ReceiverEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid receiver email address is required.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(WalletTransferConstants.Range.AmountMin)
            .WithMessage($"Minimum transfer amount is {WalletTransferConstants.Range.AmountMin} EUR.")
            .LessThanOrEqualTo(WalletTransferConstants.Range.AmountMax)
            .WithMessage($"Maximum transfer amount is {WalletTransferConstants.Range.AmountMax} EUR.");

        RuleFor(x => x.Note)
            .MaximumLength(WalletTransferConstants.Lengths.NoteMax)
            .When(x => x.Note != null);
    }
}
