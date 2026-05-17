namespace MomVibe.Application.DTOs.Admin;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? TargetId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
