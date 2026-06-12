namespace MomVibe.Application.Validators.Business;

using FluentValidation;

using DTOs.Business;

/// <summary>FluentValidation rules for <see cref="UpdateBusinessProfileRequest"/>.</summary>
public class UpdateBusinessProfileValidator : AbstractValidator<UpdateBusinessProfileRequest>
{
    public UpdateBusinessProfileValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Bio).MaximumLength(2000);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.ContactPhone).MaximumLength(32);
        RuleFor(x => x.Website).MaximumLength(2048);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
    }
}
