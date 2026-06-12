namespace MomVibe.Application.Validators.Business;

using FluentValidation;

using DTOs.Business;

/// <summary>
/// FluentValidation rules for <see cref="SubmitCoachReferralRequest"/>. At least one of
/// email/phone is required so admins have a contact channel.
/// </summary>
public class SubmitCoachReferralValidator : AbstractValidator<SubmitCoachReferralRequest>
{
    public SubmitCoachReferralValidator()
    {
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.ReferralCode).MaximumLength(16);
        RuleFor(x => x.ContactEmail).MaximumLength(254).EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(32);
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.ContactEmail) || !string.IsNullOrWhiteSpace(x.ContactPhone))
            .WithMessage("Either an email or a phone number is required so we can reach the coach.");
    }
}
