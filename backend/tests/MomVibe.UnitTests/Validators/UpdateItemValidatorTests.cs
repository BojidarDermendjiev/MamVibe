using FluentValidation.TestHelper;

using MomVibe.Application.DTOs.Items;
using MomVibe.Application.Validators;
using MomVibe.Domain.Enums;

namespace MomVibe.UnitTests.Validators;

public class UpdateItemValidatorTests
{
    private readonly UpdateItemValidator _validator = new();

    // =========================================================================
    // Title rules
    // =========================================================================

    [Fact]
    public void Should_Pass_When_No_Fields_Are_Provided()
    {
        // A fully-null DTO is a valid partial-update request
        var dto = new UpdateItemDto();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Title_Is_Within_MaxLength()
    {
        var dto = new UpdateItemDto { Title = new string('a', 200) };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_When_Title_Exceeds_200_Characters()
    {
        var dto = new UpdateItemDto { Title = new string('x', 201) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Not_Validate_Title_When_Title_Is_Null()
    {
        // When Title is null the conditional rule must not trigger
        var dto = new UpdateItemDto { Title = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    // =========================================================================
    // Description rules
    // =========================================================================

    [Fact]
    public void Should_Pass_When_Description_Is_Within_MaxLength()
    {
        var dto = new UpdateItemDto { Description = new string('d', 5000) };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_5000_Characters()
    {
        var dto = new UpdateItemDto { Description = new string('d', 5001) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Validate_Description_When_Description_Is_Null()
    {
        var dto = new UpdateItemDto { Description = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // =========================================================================
    // Price rules (conditional on ListingType == Sell)
    // =========================================================================

    [Fact]
    public void Should_Pass_When_Sell_Listing_Has_Positive_Price()
    {
        var dto = new UpdateItemDto { ListingType = ListingType.Sell, Price = 10m };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Fail_When_Sell_Listing_Price_Is_Zero()
    {
        var dto = new UpdateItemDto { ListingType = ListingType.Sell, Price = 0m };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Fail_When_Sell_Listing_Price_Is_Negative()
    {
        var dto = new UpdateItemDto { ListingType = ListingType.Sell, Price = -5m };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Not_Validate_Price_When_ListingType_Is_Donate()
    {
        // Donate listings are free; a zero price must not be flagged
        var dto = new UpdateItemDto { ListingType = ListingType.Donate, Price = 0m };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Not_Validate_Price_When_Price_Is_Null()
    {
        // No price provided — conditional rule does not fire
        var dto = new UpdateItemDto { ListingType = ListingType.Sell, Price = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Pass_With_Combined_Valid_Fields()
    {
        var dto = new UpdateItemDto
        {
            Title = "Updated Stroller",
            Description = "Slightly used, excellent condition",
            ListingType = ListingType.Sell,
            Price = 75m,
            IsActive = true,
            PhotoUrls = ["https://example.com/new.jpg"]
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
