namespace MomVibe.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using Common;
using Enums;

/// <summary>
/// Immutable record of an admin moderation action (Approve or Delete) on a listing.
/// Written at the moment the action is taken; never updated after creation.
/// </summary>
public class ItemModerationLog : BaseEntity
{
    [Required]
    public Guid ItemId { get; set; }

    [Required]
    public required string AdminId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string AdminDisplayName { get; set; }

    public ModerationAction Action { get; set; }

    /// <summary>The AI moderation status the item had at the time of the action.</summary>
    public AiModerationStatus AiStatusAtTime { get; set; }

    [MaxLength(500)]
    public string? AiNotesAtTime { get; set; }

    [MaxLength(200)]
    public required string ItemTitle { get; set; }
}
