namespace MomVibe.Application.Interfaces;

using MomVibe.Application.DTOs.Moderation;

/// <summary>
/// Background-friendly heuristics that flag user accounts for admin review based on observed
/// activity (failed-login bursts, mass listing creation, spam keywords, multi-account from the
/// same IP). All <c>Record*</c>/<c>Evaluate*</c> methods are fire-and-forget from the caller's
/// perspective — they catch their own exceptions and never throw, so a misfiring detector
/// cannot break the originating request.
/// </summary>
/// <remarks>
/// No method ever applies a moderation action. Detections write <c>AbuseSignal</c> rows that
/// flow into the admin abuse-signals queue.
/// </remarks>
public interface IAbuseDetectionService
{
    Task RecordFailedLoginAsync(string? userIdOrEmail, string? ipAddress);
    Task EvaluateListingBurstAsync(string userId);
    Task EvaluateMessageAsync(string senderId, string content);
    Task EvaluateMultiAccountAsync(string newUserId, string? ipAddress);

    Task<PagedModerationResult<AbuseSignalDto>> GetAdminQueueAsync(AbuseSignalFilter filter);
    Task AcknowledgeAsync(Guid signalId, string adminId);
}
