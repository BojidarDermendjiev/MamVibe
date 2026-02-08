namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Feedbacks;

/// <summary>
/// Validator for CreateFeedbackDto: enforces non-empty content (max 2000 characters),
/// rating within the inclusive range 1–5, and a valid Category enum value.
/// Implements rules with FluentValidation for concise, maintainable input validation.
/// </summary>
public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackDto>
{
    public CreateFeedbackValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Category).IsInEnum();
    }
}
