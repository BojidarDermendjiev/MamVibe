namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Wallet;

public class RefundRequestValidator : AbstractValidator<RefundRequestDto>
{
    public RefundRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("A reason must be provided for the refund.")
            .MaximumLength(500);
    }
}
