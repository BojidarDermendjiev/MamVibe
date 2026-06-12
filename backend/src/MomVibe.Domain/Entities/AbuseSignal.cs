namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using Common;
using Enums;

/// <summary>
/// Auto-detected abuse signal raised by background heuristics (failed-login burst, mass listing,
/// spam keywords, multi-account same IP, report threshold). Signals never auto-enforce —
/// they flag the subject user for admin review via the admin abuse-signals queue.
/// </summary>
public class AbuseSignal : BaseEntity
{
    /// <summary>The signal-source heuristic that fired.</summary>
    public AbuseSignalType Type { get; set; }

    /// <summary>Identifier of the user the signal is about (FK to ApplicationUser.Id).</summary>
    [Required]
    public required string SubjectUserId { get; set; }

    /// <summary>Severity weight 1–100 — summed in the admin queue for prioritisation.</summary>
    public int Score { get; set; }

    /// <summary>Optional JSON payload with detection metadata (e.g., <c>{"failedAttempts":7,"window":"5m","ip":"…"}</c>).</summary>
    [MaxLength(2000)]
    public string? Details { get; set; }

    /// <summary>Optional pointer to a piece of evidence (item id, message id, etc.).</summary>
    [MaxLength(450)]
    public string? EvidenceTargetId { get; set; }

    /// <summary>True once an admin has reviewed and acknowledged this signal.</summary>
    public bool Acknowledged { get; set; }

    /// <summary>Identifier of the admin who acknowledged.</summary>
    [MaxLength(450)]
    public string? AcknowledgedByAdminId { get; set; }

    /// <summary>UTC timestamp when acknowledged.</summary>
    public DateTime? AcknowledgedAt { get; set; }
}
