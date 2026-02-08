namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Auth;

/// <summary>
/// Validator for RegisterRequestDto using FluentValidation:
/// - Email: required and must be a valid email address.
/// - Password: required, minimum 8 characters, must contain at least one uppercase letter,
///   one lowercase letter, and one digit.
/// - ConfirmPassword: must match Password (custom message on mismatch).
/// - DisplayName: required, maximum 100 characters.
/// - ProfileType: must be a valid enum value.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProfileType).IsInEnum();
    }
}
