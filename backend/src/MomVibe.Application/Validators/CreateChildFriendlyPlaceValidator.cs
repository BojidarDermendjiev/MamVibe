namespace MomVibe.Application.Validators;

using FluentValidation;
using DTOs.ChildFriendlyPlaces;

/// <summary>
/// Validates <see cref="CreateChildFriendlyPlaceDto"/> using FluentValidation:
/// - Name: required, max 150 characters.
/// - Description: required, max 2000 characters.
/// - City: required, max 100 characters.
/// - AgeFromMonths/AgeToMonths: non-negative and ordered when provided.
/// - PhotoUrl/Website: max 2048 characters.
/// </summary>
public class CreateChildFriendlyPlaceValidator : AbstractValidator<CreateChildFriendlyPlaceDto>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CreateChildFriendlyPlaceValidator"/> and registers all validation rules.
    /// </summary>
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
