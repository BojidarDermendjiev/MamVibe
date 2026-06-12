namespace MomVibe.Application.Validators.Business;

using FluentValidation;

using DTOs.Business;

/// <summary>
/// FluentValidation rules for <see cref="ReportBusinessListingRequest"/>. Matches the
/// shape enforced by the underlying <c>SubmitReportRequest</c> (10–2000 char description).
/// </summary>
public class ReportBusinessListingValidator : AbstractValidator<ReportBusinessListingRequest>
{
    public ReportBusinessListingValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10).MaximumLength(2000);
    }
}
