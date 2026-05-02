namespace MomVibe.Application.Validators;

using FluentValidation;
using DTOs.ChildFriendlyPlaces;

public class CreateChildFriendlyPlaceValidator : AbstractValidator<CreateChildFriendlyPlaceDto>
{
    public CreateChildFriendlyPlaceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Address).MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AgeFromMonths).GreaterThanOrEqualTo(0).When(x => x.AgeFromMonths.HasValue);
        RuleFor(x => x.AgeToMonths).GreaterThan(x => x.AgeFromMonths ?? 0).When(x => x.AgeToMonths.HasValue);
        RuleFor(x => x.PhotoUrl).MaximumLength(2048);
        RuleFor(x => x.Website).MaximumLength(2048);
    }
}
