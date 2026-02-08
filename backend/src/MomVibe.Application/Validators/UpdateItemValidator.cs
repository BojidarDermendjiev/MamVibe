namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Items;
using Domain.Enums;

/// <summary>
/// Validator for UpdateItemDto using FluentValidation:
/// - Title: max 200 characters when provided.
/// - Description: max 5000 characters when provided.
/// - Price: must be > 0 when provided and ListingType is Sell.
/// Applies conditional rules to support partial updates.
/// </summary>
public class UpdateItemValidator : AbstractValidator<UpdateItemDto>
{
    public UpdateItemValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title != null);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description != null);
        RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue && x.ListingType == ListingType.Sell);
    }
}
