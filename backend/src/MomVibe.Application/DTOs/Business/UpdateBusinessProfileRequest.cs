namespace MomVibe.Application.DTOs.Business;

using Domain.Enums;

/// <summary>
/// Payload for editing the editable parts of an existing <c>BusinessProfile</c>.
/// Stripe customer id, fingerprint hash, status, and policy acceptance are NOT editable here.
/// </summary>
public class UpdateBusinessProfileRequest
{
    public ProfileKind ProfileKind { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }
    public string City { get; set; } = string.Empty;
}
