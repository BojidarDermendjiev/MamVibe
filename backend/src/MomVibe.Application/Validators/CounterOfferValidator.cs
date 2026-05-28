namespace MomVibe.Application.Validators;

using FluentValidation;
using DTOs.Offers;

public class CounterOfferValidator : AbstractValidator<CounterOfferDto>
{
    public CounterOfferValidator()
    {
        RuleFor(x => x.CounterPrice).GreaterThan(0).LessThanOrEqualTo(999_999);
    }
}
