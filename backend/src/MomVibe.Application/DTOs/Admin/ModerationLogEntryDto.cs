namespace MomVibe.Application.DTOs.Admin;

public class ModerationLogEntryDto
{
    public string AdminDisplayName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string AiStatusAtTime { get; set; } = string.Empty;
    public string? AiNotesAtTime { get; set; }
    public DateTime Timestamp { get; set; }
}
