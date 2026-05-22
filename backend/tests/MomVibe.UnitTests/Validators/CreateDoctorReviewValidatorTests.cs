using FluentValidation.TestHelper;

using MomVibe.Application.DTOs.DoctorReviews;
using MomVibe.Application.Validators;

namespace MomVibe.UnitTests.Validators;

/// <summary>
/// Validates <see cref="CreateDoctorReviewValidator"/> rules:
/// DoctorName, Specialization, City (required + max 100),
/// ClinicName (optional max 150), Rating (1–5),
/// Content (required max 2000), SuperdocUrl (optional, superdoc.bg domain).
/// </summary>
public class CreateDoctorReviewValidatorTests
{
    private readonly CreateDoctorReviewValidator _validator = new();

    // =========================================================================
    // Happy path
    // =========================================================================

    [Fact]
    public void Should_Pass_With_All_Valid_Fields()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Maria Ivanova",
            Specialization = "Pediatrics",
            ClinicName = "City Medical Center",
            City = "Sofia",
            Rating = 5,
            Content = "Excellent doctor — very attentive and thorough.",
            SuperdocUrl = "https://superdoc.bg/doctors/maria-ivanova"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_Without_Optional_Fields()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Ivan Petrov",
            Specialization = "Cardiology",
            City = "Plovdiv",
            Rating = 4,
            Content = "Good experience overall."
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // =========================================================================
    // DoctorName rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_DoctorName_Is_Empty()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = "OK visit."
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DoctorName);
    }

    [Fact]
    public void Should_Fail_When_DoctorName_Exceeds_100_Characters()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = new string('D', 101),
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = "OK visit."
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DoctorName);
    }

    // =========================================================================
    // Specialization rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Specialization_Is_Empty()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "",
            City = "Sofia",
            Rating = 3,
            Content = "OK."
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Specialization);
    }

    [Fact]
    public void Should_Fail_When_Specialization_Exceeds_100_Characters()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = new string('S', 101),
            City = "Sofia",
            Rating = 3,
            Content = "OK."
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Specialization);
    }

    // =========================================================================
    // City rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_City_Is_Empty()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "",
            Rating = 3,
            Content = "OK."
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    // =========================================================================
    // ClinicName rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_ClinicName_Exceeds_150_Characters()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            ClinicName = new string('C', 151),
            Rating = 3,
            Content = "OK."
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ClinicName);
    }

    // =========================================================================
    // Rating rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Rating_Is_Zero()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 0,
            Content = "Below minimum."
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Should_Fail_When_Rating_Exceeds_5()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 6,
            Content = "Exceeds max."
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Should_Pass_For_Boundary_Ratings_One_And_Five()
    {
        foreach (var rating in new[] { 1, 5 })
        {
            var dto = new CreateDoctorReviewDto
            {
                DoctorName = "Dr. Test",
                Specialization = "Pediatrics",
                City = "Sofia",
                Rating = rating,
                Content = "Boundary test."
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Rating);
        }
    }

    // =========================================================================
    // Content rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_Content_Is_Empty()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = ""
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_Fail_When_Content_Exceeds_2000_Characters()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = new string('x', 2001)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    // =========================================================================
    // SuperdocUrl rules
    // =========================================================================

    [Fact]
    public void Should_Fail_When_SuperdocUrl_Is_Not_A_Superdoc_Link()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = "OK.",
            SuperdocUrl = "https://example.com/doctor"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.SuperdocUrl);
    }

    [Fact]
    public void Should_Pass_When_SuperdocUrl_Is_Valid_Superdoc_Link()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = "OK.",
            SuperdocUrl = "https://superdoc.bg/doctors/test-profile"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.SuperdocUrl);
    }

    [Fact]
    public void Should_Pass_When_SuperdocUrl_Is_Null()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = "OK.",
            SuperdocUrl = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.SuperdocUrl);
    }

    [Fact]
    public void Should_Fail_When_SuperdocUrl_Exceeds_2048_Characters()
    {
        var dto = new CreateDoctorReviewDto
        {
            DoctorName = "Dr. Test",
            Specialization = "Pediatrics",
            City = "Sofia",
            Rating = 3,
            Content = "OK.",
            SuperdocUrl = "https://superdoc.bg/" + new string('x', 2040)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.SuperdocUrl);
    }
}
