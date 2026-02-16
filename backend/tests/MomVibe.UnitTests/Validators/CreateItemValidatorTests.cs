using FluentValidation.TestHelper;
using MomVibe.Application.DTOs.Items;
using MomVibe.Application.Validators;
using MomVibe.Domain.Enums;

namespace MomVibe.UnitTests.Validators;

public class CreateItemValidatorTests
{
    private readonly CreateItemValidator _validator = new();

    [Fact]
    public void Should_Pass_With_Valid_Donate_Item()
    {
        var dto = new CreateItemDto
        {
            Title = "Baby Clothes",
            Description = "Gently used baby clothes, size 0-3 months",
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Donate,
            PhotoUrls = ["https://example.com/photo1.jpg"]
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_With_Valid_Sell_Item()
    {
        var dto = new CreateItemDto
        {
            Title = "Baby Stroller",
            Description = "Excellent condition stroller, barely used",
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Sell,
            Price = 49.99m,
            PhotoUrls = ["https://example.com/stroller.jpg"]
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Title_Is_Empty()
    {
        var dto = new CreateItemDto
        {
            Title = "",
            Description = "Some description",
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Donate
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_When_Description_Is_Empty()
    {
        var dto = new CreateItemDto
        {
            Title = "Test Item",
            Description = "",
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Donate
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Title_Too_Long()
    {
        var dto = new CreateItemDto
        {
            Title = new string('a', 201),
            Description = "Some description",
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Donate
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Fail_When_Sell_Item_Has_No_Price()
    {
        var dto = new CreateItemDto
        {
            Title = "Test Item",
            Description = "Some description",
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Sell,
            Price = null,
            PhotoUrls = ["https://example.com/photo.jpg"]
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Fail_When_Price_Is_Negative()
    {
        var dto = new CreateItemDto
        {
            Title = "Test Item",
            Description = "Some description",
            CategoryId = Guid.NewGuid(),
            ListingType = ListingType.Sell,
            Price = -5m
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
}
