namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>
/// Read-only projection of a <c>BusinessProfile</c>. Anti-abuse fields
/// (fingerprint hash, IP, UA) are intentionally NOT exposed.
/// </summary>
public class BusinessProfileDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    /// <summary>Coach (services for kids) or VenueAdvertiser (places/attractions).</summary>
    public BusinessCategory Category { get; set; }

    public ProfileKind ProfileKind { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }
    public string City { get; set; } = string.Empty;
    public BusinessProfileStatus Status { get; set; }

    /// <summary>True when the active policy version is newer than the one this profile accepted — UI must show the modal again.</summary>
    public bool PolicyReacceptanceRequired { get; set; }

    public bool HasListing { get; set; }
    public bool HasSubscription { get; set; }
    public DateTime CreatedAt { get; set; }
}
