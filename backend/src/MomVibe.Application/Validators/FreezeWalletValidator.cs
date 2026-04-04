namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Wallet;
using Domain.Constants;

public class FreezeWalletValidator : AbstractValidator<FreezeWalletDto>
{
    public FreezeWalletValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("A reason must be provided when freezing a wallet.")
            .MaximumLength(WalletConstants.Lengths.FreezeReasonMax);
    }
}
