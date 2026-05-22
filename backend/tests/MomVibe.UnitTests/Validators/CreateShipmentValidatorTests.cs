using FluentValidation.TestHelper;

using MomVibe.Application.DTOs.Shipping;
using MomVibe.Application.Validators;
using MomVibe.Domain.Enums;

namespace MomVibe.UnitTests.Validators;

/// <summary>
/// Validates <see cref="CreateShipmentValidator"/> rules:
/// PaymentId, CourierProvider, DeliveryType, RecipientName, RecipientPhone, Weight,
/// conditional DeliveryAddress+City (Address delivery), OfficeId (Office/Locker delivery),
/// conditional CodAmount, conditional InsuredAmount.
/// </summary>
public class CreateShipmentValidatorTests
{
    private readonly CreateShipmentValidator _validator = new();

    // =========================================================================
    // Happy path
    // =========================================================================

    [Fact]
    public void Should_Pass_For_Valid_Office_Delivery()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Maria Petrova",
            RecipientPhone = "0888111222",
            Weight = 1.5m,
            OfficeId = "OFF-001"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_For_Valid_Address_Delivery()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Speedy,
            DeliveryType = DeliveryType.Address,
            RecipientName = "Ivan Georgiev",
            RecipientPhone = "0877333444",
            Weight = 0.8m,
            DeliveryAddress = "ul. Rakovski 15",
            City = "Sofia"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_For_Valid_Locker_Delivery()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.BoxNow,
            DeliveryType = DeliveryType.Locker,
            RecipientName = "Georgi Nikolov",
            RecipientPhone = "0866555666",
            Weight = 0.5m,
            OfficeId = "BN-001"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // =========================================================================
    // PaymentId rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_PaymentId_Is_Empty()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.Empty,
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = "OFF-001"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PaymentId);
    }

    // =========================================================================
    // RecipientName / RecipientPhone
    // =========================================================================

    [Fact]
    public void Should_Fail_When_RecipientName_Is_Empty()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Speedy,
            DeliveryType = DeliveryType.Office,
            RecipientName = "",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = "OFF-001"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.RecipientName);
    }

    [Fact]
    public void Should_Fail_When_RecipientPhone_Is_Empty()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Speedy,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "",
            Weight = 1m,
            OfficeId = "OFF-001"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.RecipientPhone);
    }

    // =========================================================================
    // Weight rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Weight_Is_Zero()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 0m,
            OfficeId = "OFF-001"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Weight);
    }

    [Fact]
    public void Should_Fail_When_Weight_Is_Negative()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = -1m,
            OfficeId = "OFF-001"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Weight);
    }

    // =========================================================================
    // Address delivery — conditional rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_DeliveryAddress_Is_Empty_For_Address_Delivery()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Address,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            City = "Sofia",
            DeliveryAddress = ""
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress);
    }

    [Fact]
    public void Should_Fail_When_City_Is_Empty_For_Address_Delivery()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Address,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            DeliveryAddress = "ul. Test 1",
            City = ""
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Should_Not_Require_DeliveryAddress_For_Office_Delivery()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = "OFF-001"
            // DeliveryAddress intentionally omitted
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.DeliveryAddress);
    }

    // =========================================================================
    // Office/Locker delivery — conditional OfficeId
    // =========================================================================

    [Fact]
    public void Should_Fail_When_OfficeId_Is_Empty_For_Locker_Delivery()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.BoxNow,
            DeliveryType = DeliveryType.Locker,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = ""
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.OfficeId);
    }

    // =========================================================================
    // COD rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_IsCod_True_And_CodAmount_Is_Zero()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = "OFF-001",
            IsCod = true,
            CodAmount = 0m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CodAmount);
    }

    [Fact]
    public void Should_Pass_When_IsCod_True_And_CodAmount_Is_Positive()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = "OFF-001",
            IsCod = true,
            CodAmount = 25m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CodAmount);
    }

    [Fact]
    public void Should_Not_Validate_CodAmount_When_IsCod_Is_False()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = "OFF-001",
            IsCod = false,
            CodAmount = 0m  // zero is allowed when COD is disabled
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CodAmount);
    }

    // =========================================================================
    // Insurance rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_IsInsured_True_And_InsuredAmount_Is_Zero()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = "OFF-001",
            IsInsured = true,
            InsuredAmount = 0m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.InsuredAmount);
    }

    [Fact]
    public void Should_Pass_When_IsInsured_True_And_InsuredAmount_Is_Positive()
    {
        var dto = new CreateShipmentDto
        {
            PaymentId = Guid.NewGuid(),
            CourierProvider = CourierProvider.Econt,
            DeliveryType = DeliveryType.Office,
            RecipientName = "Test Recipient",
            RecipientPhone = "0888000111",
            Weight = 1m,
            OfficeId = "OFF-001",
            IsInsured = true,
            InsuredAmount = 100m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.InsuredAmount);
    }
}
