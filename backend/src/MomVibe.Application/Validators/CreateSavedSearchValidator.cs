namespace MomVibe.Application.Validators;

using FluentValidation;
using DTOs.SavedSearches;

public class CreateSavedSearchValidator : AbstractValidator<CreateSavedSearchDto>
{
    public CreateSavedSearchValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SearchTerm).MaximumLength(200).When(x => x.SearchTerm != null);
        RuleFor(x => x.MaxPrice).GreaterThan(0).LessThanOrEqualTo(999_999).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x.ShoeSize).GreaterThan(0).LessThanOrEqualTo(60).When(x => x.ShoeSize.HasValue);
        RuleFor(x => x.ClothingSize).GreaterThan(0).LessThanOrEqualTo(200).When(x => x.ClothingSize.HasValue);
    }
}
