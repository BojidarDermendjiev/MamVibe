using FluentValidation.TestHelper;

using MomVibe.Application.DTOs.Feedbacks;
using MomVibe.Application.Validators;
using MomVibe.Domain.Enums;

namespace MomVibe.UnitTests.Validators;

public class CreateFeedbackValidatorTests
{
    private readonly CreateFeedbackValidator _validator = new();

    // =========================================================================
    // Content rules
    // =========================================================================

    [Fact]
    public void Should_Pass_With_Valid_Feedback()
    {
        var dto = new CreateFeedbackDto
        {
            Content = "Great experience overall!",
            Rating = 5,
            Category = FeedbackCategory.Praise,
            IsContactable = false
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Content_Is_Empty()
    {
        var dto = new CreateFeedbackDto
        {
            Content = "",
            Rating = 3,
            Category = FeedbackCategory.Improvement
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_Fail_When_Content_Exceeds_2000_Characters()
    {
        var dto = new CreateFeedbackDto
        {
            Content = new string('a', 2001),
            Rating = 4,
            Category = FeedbackCategory.BugReport
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_Pass_When_Content_Is_At_Exactly_2000_Characters()
    {
        var dto = new CreateFeedbackDto
        {
            Content = new string('a', 2000),
            Rating = 4,
            Category = FeedbackCategory.FeatureRequest
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    // =========================================================================
    // Rating rules
    // =========================================================================

    [Fact]
    public void Should_Pass_With_Minimum_Rating_Of_1()
    {
        var dto = new CreateFeedbackDto { Content = "Minimum rating", Rating = 1, Category = FeedbackCategory.Praise };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Should_Pass_With_Maximum_Rating_Of_5()
    {
        var dto = new CreateFeedbackDto { Content = "Maximum rating", Rating = 5, Category = FeedbackCategory.Praise };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Should_Fail_When_Rating_Is_Zero()
    {
        var dto = new CreateFeedbackDto { Content = "Zero rating", Rating = 0, Category = FeedbackCategory.Improvement };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Should_Fail_When_Rating_Is_Negative()
    {
        var dto = new CreateFeedbackDto { Content = "Negative rating", Rating = -1, Category = FeedbackCategory.BugReport };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Should_Fail_When_Rating_Exceeds_5()
    {
        var dto = new CreateFeedbackDto { Content = "Too high rating", Rating = 6, Category = FeedbackCategory.FeatureRequest };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    // =========================================================================
    // Category rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Category_Is_Invalid_Enum_Value()
    {
        var dto = new CreateFeedbackDto
        {
            Content = "Some feedback",
            Rating = 3,
            Category = (FeedbackCategory)9999
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public void Should_Pass_With_IsContactable_True()
    {
        var dto = new CreateFeedbackDto
        {
            Content = "Please contact me",
            Rating = 4,
            Category = FeedbackCategory.Improvement,
            IsContactable = true
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
