namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Users;
using Domain.Constants;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(ApplicationUserConstants.Lengths.DisplayNameMax)
            .When(x => x.DisplayName != null);

        RuleFor(x => x.Bio)
            .MaximumLength(ApplicationUserConstants.Lengths.BioMax)
            .When(x => x.Bio != null);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(ApplicationUserConstants.Lengths.AvatarUrlMax)
            .Must(url => url == null || (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                                        && Uri.TryCreate(url, UriKind.Absolute, out _)))
            .WithMessage("AvatarUrl must be a valid https:// URL.")
            .When(x => x.AvatarUrl != null);

        RuleFor(x => x.LanguagePreference)
            .Must(lang => lang == null || lang is "en" or "bg")
            .WithMessage("LanguagePreference must be 'en' or 'bg'.")
            .When(x => x.LanguagePreference != null);

        RuleFor(x => x.RevolutTag)
            .MaximumLength(ApplicationUserConstants.Lengths.RevolutTagMax)
            .When(x => x.RevolutTag != null);
    }
}
