namespace MomVibe.Application.Validators;

using FluentValidation;
using DTOs.Offers;

public class CreateOfferValidator : AbstractValidator<CreateOfferDto>
{
    public CreateOfferValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.OfferedPrice).GreaterThan(0).LessThanOrEqualTo(999_999);
    }
}
