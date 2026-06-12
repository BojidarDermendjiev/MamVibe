namespace MomVibe.Application.Validators.Business;

using FluentValidation;

using DTOs.Business;

/// <summary>FluentValidation rules for <see cref="UpdateBusinessListingRequest"/> (mirror create).</summary>
public class UpdateBusinessListingValidator : AbstractValidator<UpdateBusinessListingRequest>
{
    public UpdateBusinessListingValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressLine).MaximumLength(300);
        RuleFor(x => x.Schedule).MaximumLength(500);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).When(x => x.Longitude.HasValue);
        RuleFor(x => x.Latitude)
            .NotNull().When(x => x.Longitude.HasValue)
            .WithMessage("Latitude is required when longitude is provided.");
        RuleFor(x => x.Longitude)
            .NotNull().When(x => x.Latitude.HasValue)
            .WithMessage("Longitude is required when latitude is provided.");

        RuleFor(x => x.AgeFromMonths)
            .InclusiveBetween((short)0, (short)216).When(x => x.AgeFromMonths.HasValue);
        RuleFor(x => x.AgeToMonths)
            .GreaterThan(x => x.AgeFromMonths ?? (short)0).When(x => x.AgeToMonths.HasValue);
        RuleFor(x => x.AgeToMonths)
            .LessThanOrEqualTo((short)216).When(x => x.AgeToMonths.HasValue);

        RuleFor(x => x.PriceFromEur)
            .GreaterThanOrEqualTo(0m).When(x => x.PriceFromEur.HasValue);

        RuleFor(x => x.PhotoUrls).NotNull();
        RuleForEach(x => x.PhotoUrls).MaximumLength(500);
        RuleFor(x => x.PhotoUrls.Count)
            .LessThanOrEqualTo(15)
            .WithMessage("Up to 15 photos per listing.");
    }
}
