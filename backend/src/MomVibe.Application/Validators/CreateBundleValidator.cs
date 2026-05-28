namespace MomVibe.Application.Validators;

using FluentValidation;
using DTOs.Bundles;

public class CreateBundleValidator : AbstractValidator<CreateBundleDto>
{
    public CreateBundleValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        RuleFor(x => x.Price).GreaterThan(0).LessThanOrEqualTo(999_999);
        RuleFor(x => x.ItemIds).NotEmpty()
            .Must(ids => ids.Count >= 2).WithMessage("A bundle must contain at least 2 items.")
            .Must(ids => ids.Count <= 10).WithMessage("A bundle cannot contain more than 10 items.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Duplicate items are not allowed in a bundle.");
    }
}
