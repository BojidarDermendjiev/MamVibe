namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>
/// Payload submitted when a user creates a new <c>BusinessProfile</c>. Includes the
/// FingerprintJS visitor id (raw — hashed server-side) and the id of the policy version
/// the user accepted in the modal.
/// </summary>
public class CreateBusinessProfileRequest
{
    /// <summary>Coach (services for kids) or VenueAdvertiser (places/attractions).</summary>
    public BusinessCategory Category { get; set; } = BusinessCategory.Coach;

    public ProfileKind ProfileKind { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }
    public string City { get; set; } = string.Empty;

    /// <summary>Id of the <c>BusinessPolicyVersion</c> the user just accepted in the modal.</summary>
    public Guid PolicyVersionId { get; set; }

    /// <summary>
    /// Raw FingerprintJS visitor id from the client. Hashed (SHA-256) server-side and
    /// never persisted in its raw form. Required.
    /// </summary>
    public string FingerprintVisitorId { get; set; } = string.Empty;
}
