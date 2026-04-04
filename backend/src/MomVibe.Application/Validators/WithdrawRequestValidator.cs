namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Wallet;
using Domain.Constants;

public class WithdrawRequestValidator : AbstractValidator<WithdrawRequestDto>
{
    public WithdrawRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(1m)
            .WithMessage("Minimum withdrawal amount is 1 EUR.")
            .LessThanOrEqualTo(WalletTransactionConstants.Range.AmountMax)
            .WithMessage($"Maximum withdrawal amount is {WalletTransactionConstants.Range.AmountMax} EUR.");
    }
}
