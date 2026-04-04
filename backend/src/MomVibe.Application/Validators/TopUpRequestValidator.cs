namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Wallet;
using Domain.Constants;

public class TopUpRequestValidator : AbstractValidator<TopUpRequestDto>
{
    public TopUpRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(WalletTransactionConstants.Range.AmountMin)
            .WithMessage($"Minimum top-up amount is {WalletTransactionConstants.Range.AmountMin} EUR.")
            .LessThanOrEqualTo(5000m)
            .WithMessage("Maximum top-up amount is 5000 EUR.");
    }
}
