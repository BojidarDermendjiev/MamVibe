namespace MomVibe.UnitTests.Validators;

using FluentValidation.TestHelper;

using Domain.Enums;
using Application.DTOs.Auth;
using Application.Validators;
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_With_Valid_Data()
    {
        var dto = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!",
            DisplayName = "Test User",
            ProfileType = ProfileType.Female
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Empty()
    {
        var dto = new RegisterRequestDto
        {
            Email = "",
            Password = "Password1!",
            ConfirmPassword = "Password1!",
            DisplayName = "Test",
            ProfileType = ProfileType.Male
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_When_Password_Too_Short()
    {
        var dto = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "Pass1!",
            ConfirmPassword = "Pass1!",
            DisplayName = "Test",
            ProfileType = ProfileType.Male
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Passwords_Dont_Match()
    {
        var dto = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "Password1!",
            ConfirmPassword = "Different1!",
            DisplayName = "Test",
            ProfileType = ProfileType.Male
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Should_Fail_When_DisplayName_Is_Empty()
    {
        var dto = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!",
            DisplayName = "",
            ProfileType = ProfileType.Male
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Fact]
    public void Should_Fail_When_Password_Has_No_Uppercase()
    {
        var dto = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "password1!",
            ConfirmPassword = "password1!",
            DisplayName = "Test",
            ProfileType = ProfileType.Male
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Has_No_Digit()
    {
        var dto = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "PasswordABC!",
            ConfirmPassword = "PasswordABC!",
            DisplayName = "Test",
            ProfileType = ProfileType.Male
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
