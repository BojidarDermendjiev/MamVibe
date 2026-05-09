namespace MomVibe.Application.DTOs.DoctorReviews;

/// <summary>
/// Read-only projection of a <see cref="Domain.Entities.DoctorReview"/> returned by the API.
/// </summary>
public class DoctorReviewDto
{
    /// <summary>Gets or sets the unique identifier of the review.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who submitted the review.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the author, or <c>null</c> when the review is anonymous.</summary>
    public string? AuthorDisplayName { get; set; }

    /// <summary>Gets or sets the avatar URL of the author, or <c>null</c> when the review is anonymous.</summary>
    public string? AuthorAvatarUrl { get; set; }

    /// <summary>Gets or sets the full name of the doctor being reviewed.</summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Gets or sets the medical specialization of the doctor.</summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the clinic or hospital where the doctor practices.</summary>
    public string? ClinicName { get; set; }

    /// <summary>Gets or sets the city where the doctor's practice is located.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the numeric rating on a 1–5 scale.</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the full text content of the review.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional link to the doctor's profile on Superdoc.</summary>
    public string? SuperdocUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the review is published without the author's identity.</summary>
    public bool IsAnonymous { get; set; }

    /// <summary>Gets or sets a value indicating whether the review has been approved by an administrator.</summary>
    public bool IsApproved { get; set; }

    /// <summary>Gets or sets the UTC date and time when the review was submitted.</summary>
    public DateTime CreatedAt { get; set; }
}
