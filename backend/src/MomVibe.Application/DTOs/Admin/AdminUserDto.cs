namespace MomVibe.Application.DTOs.Admin;

using Domain.Enums;

/// <summary>
/// Admin-focused projection of a user:
/// - Identity and contact: Id, Email, DisplayName, PhoneNumber, AvatarUrl.
/// - Account status: ProfileType, IsBlocked, CreatedAt.
/// - Inventory metrics: ItemCount.
/// - Authorization: Roles (initialized to an empty list).
/// Suitable for admin user listings, detail views, and management dashboards.
/// </summary>
public class AdminUserDto
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public required string Id { get; set; }

    /// <summary>Gets or sets the email address of the user.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the public display name of the user.</summary>
    public required string DisplayName { get; set; }

    /// <summary>Gets or sets the profile type of the user.</summary>
    public ProfileType ProfileType { get; set; }

    /// <summary>Gets or sets the URL of the user's avatar image, or <c>null</c> if not set.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the phone number of the user, or <c>null</c> if not provided.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets a value indicating whether the user's account is currently blocked.</summary>
    public bool IsBlocked { get; set; }

    /// <summary>Gets or sets the total number of items listed by the user.</summary>
    public int ItemCount { get; set; }

    /// <summary>Gets or sets the UTC date and time when the user account was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the list of authorization roles assigned to the user.</summary>
    public List<string> Roles { get; set; } = [];
}
