namespace MomVibe.Application.Interfaces;

using DTOs.Business;

/// <summary>
/// Manage the single <c>BusinessProfile</c> owned by the current user. Profile creation
/// performs device-fingerprint duplicate detection, role assignment (<c>Business</c>),
/// and writes the initial policy acceptance row in one transaction.
/// </summary>
public interface IBusinessProfileService
{
    /// <summary>Returns the calling user's profile, or null if they have not created one yet.</summary>
    Task<BusinessProfileDto?> GetMineAsync(string userId);

    /// <summary>Creates the calling user's profile. Throws on duplicate device, missing policy version, or existing profile.</summary>
    Task<BusinessProfileDto> CreateAsync(string userId, CreateBusinessProfileRequest request, string? ip, string? userAgent);

    /// <summary>Updates editable fields on the calling user's profile.</summary>
    Task<BusinessProfileDto> UpdateAsync(string userId, UpdateBusinessProfileRequest request);
}
