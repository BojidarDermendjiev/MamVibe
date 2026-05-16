namespace MomVibe.Infrastructure.Services;

using MomVibe.Application.Interfaces;
using MomVibe.Domain.Entities;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;

    public AuditLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        string userId,
        string action,
        bool success,
        string? targetId = null,
        string? ipAddress = null,
        string? details = null)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Success = success,
            TargetId = targetId,
            IpAddress = ipAddress,
            Details = details,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
    }
}
