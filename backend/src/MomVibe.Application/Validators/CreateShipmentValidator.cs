namespace MomVibe.Application.Validators;

using FluentValidation;

using Domain.Enums;
using DTOs.Shipping;

/// <summary>
/// Validator for CreateShipmentDto using FluentValidation:
/// - PaymentId: required.
/// - CourierProvider, DeliveryType: must be valid enum values.
/// - RecipientName, RecipientPhone: required.
/// - DeliveryAddress, City: required when DeliveryType is Address.
/// - OfficeId: required when DeliveryType is Office or Locker.
/// - Weight: must be greater than zero.
/// - CodAmount: required and positive when IsCod is true.
/// - InsuredAmount: required and positive when IsInsured is true.
/// </summary>
public class CreateShipmentValidator : AbstractValidator<CreateShipmentDto>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CreateShipmentValidator"/> and registers all validation rules.
    /// </summary>
    public CreateShipmentValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.CourierProvider).IsInEnum();
        RuleFor(x => x.DeliveryType).IsInEnum();
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RecipientPhone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Weight must be greater than zero.");

        RuleFor(x => x.DeliveryAddress).NotEmpty()
            .When(x => x.DeliveryType == DeliveryType.Address)
            .WithMessage("Delivery address is required for address delivery.");

        RuleFor(x => x.City).NotEmpty()
            .When(x => x.DeliveryType == DeliveryType.Address)
            .WithMessage("City is required for address delivery.");

        RuleFor(x => x.OfficeId).NotEmpty()
            .When(x => x.DeliveryType == DeliveryType.Office || x.DeliveryType == DeliveryType.Locker)
            .WithMessage("Office selection is required for office or locker delivery.");

        RuleFor(x => x.CodAmount).GreaterThan(0)
            .When(x => x.IsCod)
            .WithMessage("COD amount must be greater than zero when COD is enabled.");

        RuleFor(x => x.InsuredAmount).GreaterThan(0)
            .When(x => x.IsInsured)
            .WithMessage("Insured amount must be greater than zero when insurance is enabled.");
    }
}
