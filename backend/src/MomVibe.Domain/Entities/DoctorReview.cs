namespace MomVibe.Domain.Entities;

using Common;

public class DoctorReview : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? ClinicName { get; set; }
    public string City { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SuperdocUrl { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsApproved { get; set; }
}
