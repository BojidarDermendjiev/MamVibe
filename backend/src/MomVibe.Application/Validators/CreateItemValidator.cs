namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Items;
using Domain.Enums;

/// <summary>
/// Validator for CreateItemDto using FluentValidation:
/// - Title: required, max 200 characters.
/// - Description: required, max 5000 characters.
/// - CategoryId: required.
/// - ListingType: must be a valid enum value.
/// - Price: required and > 0 when ListingType is Sell (custom message provided).
/// - PhotoUrls: at least one photo is required.
/// </summary>
public class CreateItemValidator : AbstractValidator<CreateItemDto>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ListingType).IsInEnum();
        RuleFor(x => x.Price).NotNull().GreaterThan(0).When(x => x.ListingType == ListingType.Sell)
            .WithMessage("Price is required for items listed for sale.");
        RuleFor(x => x.PhotoUrls).NotEmpty().WithMessage("At least one photo is required.");
    }
}
