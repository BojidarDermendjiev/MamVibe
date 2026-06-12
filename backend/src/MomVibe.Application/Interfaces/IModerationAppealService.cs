namespace MomVibe.Application.Interfaces;

using MomVibe.Application.DTOs.Moderation;

/// <summary>Lifecycle for user-submitted appeals against prior moderation actions.</summary>
public interface IModerationAppealService
{
    Task<Guid> SubmitAsync(string userId, SubmitAppealRequest request);
    Task<IReadOnlyList<AppealDto>> GetMyAppealsAsync(string userId);
    Task<PagedModerationResult<AppealSummaryDto>> GetAdminQueueAsync(AdminAppealFilter filter);
    Task<AppealDto?> GetAppealAsync(Guid id);
    Task DecideAsync(Guid appealId, DecideAppealRequest request, string adminId, string adminDisplayName);
}
