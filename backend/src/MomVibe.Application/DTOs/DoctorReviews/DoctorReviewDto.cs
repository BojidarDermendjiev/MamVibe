namespace MomVibe.Application.DTOs.DoctorReviews;

public class DoctorReviewDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? AuthorDisplayName { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? ClinicName { get; set; }
    public string City { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SuperdocUrl { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}
