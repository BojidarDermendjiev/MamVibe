namespace MomVibe.Application.Validators.Business;

using FluentValidation;

using DTOs.Business;

/// <summary>FluentValidation rules for <see cref="CreateBusinessListingCommentRequest"/>.</summary>
public class CreateBusinessListingCommentValidator : AbstractValidator<CreateBusinessListingCommentRequest>
{
    public CreateBusinessListingCommentValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(1000);
    }
}
