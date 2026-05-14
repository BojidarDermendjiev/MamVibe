namespace MomVibe.Application.DTOs.DoctorReviews;

/// <summary>
/// Payload used when a user submits a new doctor review for moderation.
/// </summary>
public class CreateDoctorReviewDto
{
    /// <summary>Gets or sets the full name of the doctor being reviewed.</summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Gets or sets the medical specialization of the doctor (e.g., "Pediatrics").</summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the clinic or hospital where the doctor practices.</summary>
    public string? ClinicName { get; set; }

    /// <summary>Gets or sets the city where the doctor's practice is located.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the numeric rating given by the reviewer, on a 1–5 scale.</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the full text content of the review.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional link to the doctor's profile on Superdoc.</summary>
    public string? SuperdocUrl { get; set; }
}
