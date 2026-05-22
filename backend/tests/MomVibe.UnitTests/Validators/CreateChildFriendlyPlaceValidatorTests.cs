using FluentValidation.TestHelper;

using MomVibe.Application.DTOs.ChildFriendlyPlaces;
using MomVibe.Application.Validators;
using MomVibe.Domain.Enums;

namespace MomVibe.UnitTests.Validators;

/// <summary>
/// Validates <see cref="CreateChildFriendlyPlaceValidator"/> rules:
/// Name, Description, City (required + max length),
/// Address (optional max length), AgeFromMonths/AgeToMonths (non-negative, ordered),
/// PhotoUrl/Website (optional max length).
/// </summary>
public class CreateChildFriendlyPlaceValidatorTests
{
    private readonly CreateChildFriendlyPlaceValidator _validator = new();

    // =========================================================================
    // Happy path
    // =========================================================================

    [Fact]
    public void Should_Pass_With_All_Valid_Fields()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Happy Kids Park",
            Description = "A safe outdoor play area for children of all ages.",
            City = "Sofia",
            PlaceType = PlaceType.Playground,
            AgeFromMonths = 12,
            AgeToMonths = 60,
            Address = "ul. Vitosha 1",
            PhotoUrl = "https://example.com/photo.jpg",
            Website = "https://happykidspark.bg"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // =========================================================================
    // Name rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var dto = new CreateChildFriendlyPlaceDto { Name = "", Description = "desc", City = "Sofia" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_150_Characters()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = new string('A', 151),
            Description = "desc",
            City = "Sofia"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Pass_When_Name_Is_Exactly_150_Characters()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = new string('A', 150),
            Description = "desc",
            City = "Sofia"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // =========================================================================
    // Description rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Description_Is_Empty()
    {
        var dto = new CreateChildFriendlyPlaceDto { Name = "Park", Description = "", City = "Sofia" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_2000_Characters()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = new string('d', 2001),
            City = "Sofia"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    // =========================================================================
    // City rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_City_Is_Empty()
    {
        var dto = new CreateChildFriendlyPlaceDto { Name = "Park", Description = "desc", City = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Should_Fail_When_City_Exceeds_100_Characters()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = "desc",
            City = new string('C', 101)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    // =========================================================================
    // Address rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Address_Exceeds_300_Characters()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = "desc",
            City = "Sofia",
            Address = new string('A', 301)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Should_Pass_When_Address_Is_Null()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = "desc",
            City = "Sofia",
            Address = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    // =========================================================================
    // Age range rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_AgeFromMonths_Is_Negative()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = "desc",
            City = "Sofia",
            AgeFromMonths = -1
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AgeFromMonths);
    }

    [Fact]
    public void Should_Fail_When_AgeToMonths_Is_Not_Greater_Than_AgeFromMonths()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = "desc",
            City = "Sofia",
            AgeFromMonths = 24,
            AgeToMonths = 12  // less than AgeFromMonths — invalid
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AgeToMonths);
    }

    [Fact]
    public void Should_Pass_When_Age_Range_Is_Valid_And_Ordered()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = "desc",
            City = "Sofia",
            AgeFromMonths = 6,
            AgeToMonths = 36
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.AgeFromMonths);
        result.ShouldNotHaveValidationErrorFor(x => x.AgeToMonths);
    }

    // =========================================================================
    // PhotoUrl and Website
    // =========================================================================

    [Fact]
    public void Should_Fail_When_PhotoUrl_Exceeds_2048_Characters()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = "desc",
            City = "Sofia",
            PhotoUrl = "https://example.com/" + new string('x', 2040)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhotoUrl);
    }

    [Fact]
    public void Should_Fail_When_Website_Exceeds_2048_Characters()
    {
        var dto = new CreateChildFriendlyPlaceDto
        {
            Name = "Park",
            Description = "desc",
            City = "Sofia",
            Website = "https://example.com/" + new string('x', 2040)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Website);
    }
}
