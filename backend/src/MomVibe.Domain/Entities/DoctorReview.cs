namespace MomVibe.Domain.Entities;

using Common;

/// <summary>
/// Represents a user-submitted review of a medical doctor.
/// Reviews must be approved by an administrator before they are publicly visible.
/// </summary>
public class DoctorReview : BaseEntity
{
    /// <summary>Gets or sets the identifier of the user who submitted this review.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the user who submitted this review.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Gets or sets the full name of the doctor being reviewed.</summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Gets or sets the medical specialization of the doctor (e.g., "Pediatrics").</summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the clinic or hospital where the doctor practices.</summary>
    public string? ClinicName { get; set; }

    /// <summary>Gets or sets the city where the doctor's practice is located.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the numeric rating given by the reviewer, typically on a 1–5 scale.</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the full text content of the review.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional link to the doctor's profile on Superdoc.</summary>
    public string? SuperdocUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the review should be displayed without the author's identity.</summary>
    public bool IsAnonymous { get; set; }

    /// <summary>Gets or sets a value indicating whether an administrator has approved this review.</summary>
    public bool IsApproved { get; set; }
}
