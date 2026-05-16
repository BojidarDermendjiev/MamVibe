namespace MomVibe.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        string userId,
        string action,
        bool success,
        string? targetId = null,
        string? ipAddress = null,
        string? details = null);
}
