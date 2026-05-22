using FluentValidation.TestHelper;

using MomVibe.Application.DTOs.Shipping;
using MomVibe.Application.Validators;
using MomVibe.Domain.Enums;

namespace MomVibe.UnitTests.Validators;

/// <summary>
/// Validates <see cref="CalculateShippingValidator"/> rules:
/// CourierProvider + DeliveryType (must be valid enums),
/// Weight (greater than zero),
/// conditional CodAmount (positive when IsCod=true),
/// conditional InsuredAmount (positive when IsInsured=true).
/// </summary>
public class CalculateShippingValidatorTests
{
    private readonly CalculateShippingValidator _validator = new();

    // =========================================================================
    // Happy path
    // =========================================================================

    [Fact]
    public void Should_Pass_With_Valid_Office_Delivery_Request()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            Weight = 1m,
            OfficeId = "OFF-001"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_With_Valid_Address_Delivery_Request()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Speedy,
            DeliveryType = DeliveryType.Address,
            Weight = 0.5m,
            ToCity = "Sofia"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_With_Cod_And_Positive_Amount()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            Weight = 1.5m,
            IsCod = true,
            CodAmount = 45m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_With_Insurance_And_Positive_Amount()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.BoxNow,
            DeliveryType = DeliveryType.Locker,
            Weight = 0.8m,
            IsInsured = true,
            InsuredAmount = 200m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // =========================================================================
    // CourierProvider enum validation
    // =========================================================================

    [Fact]
    public void Should_Fail_When_CourierProvider_Is_Invalid_Enum_Value()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = (CourierProvider)9999,
            DeliveryType = DeliveryType.Office,
            Weight = 1m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CourierProvider);
    }

    // =========================================================================
    // DeliveryType enum validation
    // =========================================================================

    [Fact]
    public void Should_Fail_When_DeliveryType_Is_Invalid_Enum_Value()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = (DeliveryType)9999,
            Weight = 1m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DeliveryType);
    }

    // =========================================================================
    // Weight rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Weight_Is_Zero()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            Weight = 0m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Weight);
    }

    [Fact]
    public void Should_Fail_When_Weight_Is_Negative()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Speedy,
            DeliveryType = DeliveryType.Address,
            Weight = -0.5m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Weight);
    }

    [Fact]
    public void Should_Pass_With_Small_Positive_Weight()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.BoxNow,
            DeliveryType = DeliveryType.Locker,
            Weight = 0.1m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Weight);
    }

    // =========================================================================
    // COD conditional rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_IsCod_True_And_CodAmount_Is_Zero()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            Weight = 1m,
            IsCod = true,
            CodAmount = 0m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CodAmount);
    }

    [Fact]
    public void Should_Fail_When_IsCod_True_And_CodAmount_Is_Negative()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            Weight = 1m,
            IsCod = true,
            CodAmount = -10m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CodAmount);
    }

    [Fact]
    public void Should_Not_Validate_CodAmount_When_IsCod_Is_False()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            Weight = 1m,
            IsCod = false,
            CodAmount = 0m  // zero is allowed when COD is not requested
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CodAmount);
    }

    // =========================================================================
    // Insurance conditional rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_IsInsured_True_And_InsuredAmount_Is_Zero()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.BoxNow,
            DeliveryType = DeliveryType.Locker,
            Weight = 0.5m,
            IsInsured = true,
            InsuredAmount = 0m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.InsuredAmount);
    }

    [Fact]
    public void Should_Not_Validate_InsuredAmount_When_IsInsured_Is_False()
    {
        var dto = new CalculateShippingDto
        {
            CourierProvider = CourierProvider.BoxNow,
            DeliveryType = DeliveryType.Locker,
            Weight = 0.5m,
            IsInsured = false,
            InsuredAmount = 0m  // zero is allowed when insurance is not requested
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.InsuredAmount);
    }
}
