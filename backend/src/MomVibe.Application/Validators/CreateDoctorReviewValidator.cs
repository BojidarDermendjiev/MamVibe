namespace MomVibe.Application.Validators;

using FluentValidation;
using DTOs.DoctorReviews;

public class CreateDoctorReviewValidator : AbstractValidator<CreateDoctorReviewDto>
{
    public CreateDoctorReviewValidator()
    {
        RuleFor(x => x.DoctorName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ClinicName).MaximumLength(150);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.SuperdocUrl).MaximumLength(2048).Must(url =>
            string.IsNullOrEmpty(url) || url.StartsWith("https://superdoc.bg/", StringComparison.OrdinalIgnoreCase)
        ).WithMessage("SuperdocUrl must be a valid superdoc.bg link.");
    }
}
