namespace MomVibe.Application.Validators;

using FluentValidation;

using DTOs.Shipping;

/// <summary>
/// Validator for CalculateShippingDto using FluentValidation:
/// - CourierProvider, DeliveryType: must be valid enum values.
/// - Weight: must be greater than zero.
/// - CodAmount: required and positive when IsCod is true.
/// - InsuredAmount: required and positive when IsInsured is true.
/// </summary>
public class CalculateShippingValidator : AbstractValidator<CalculateShippingDto>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CalculateShippingValidator"/> and registers all validation rules.
    /// </summary>
    public CalculateShippingValidator()
    {
        RuleFor(x => x.CourierProvider).IsInEnum();
        RuleFor(x => x.DeliveryType).IsInEnum();
        RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Weight must be greater than zero.");

        RuleFor(x => x.CodAmount).GreaterThan(0)
            .When(x => x.IsCod)
            .WithMessage("COD amount must be greater than zero when COD is enabled.");

        RuleFor(x => x.InsuredAmount).GreaterThan(0)
            .When(x => x.IsInsured)
            .WithMessage("Insured amount must be greater than zero when insurance is enabled.");
    }
}
