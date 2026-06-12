namespace MomVibe.Application.DTOs.Moderation;

using MomVibe.Domain.Enums;

public sealed record AbuseSignalDto(
    Guid Id,
    AbuseSignalType Type,
    string SubjectUserId,
    int Score,
    string? Details,
    string? EvidenceTargetId,
    bool Acknowledged,
    string? AcknowledgedByAdminId,
    DateTime? AcknowledgedAt,
    DateTime CreatedAt);

public sealed record AbuseSignalFilter(
    bool IncludeAcknowledged,
    AbuseSignalType? Type,
    int Page,
    int PageSize);
