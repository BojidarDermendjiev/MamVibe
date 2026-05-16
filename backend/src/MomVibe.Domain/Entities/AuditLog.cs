namespace MomVibe.Domain.Entities;

using Common;

/// <summary>
/// Immutable security audit record. Written on login, admin actions, and payments.
/// Never updated — append-only by convention.
/// </summary>
public class AuditLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    /// <summary>Dot-separated action name, e.g. "Auth.Login", "Admin.BlockUser".</summary>
    public string Action { get; set; } = string.Empty;

    public bool Success { get; set; }

    /// <summary>ID of the entity acted upon (blocked user, approved item, etc.).</summary>
    public string? TargetId { get; set; }

    public string? IpAddress { get; set; }

    public string? Details { get; set; }
}
