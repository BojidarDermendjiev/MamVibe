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
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public ProfileType ProfileType { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsBlocked { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = [];
}
