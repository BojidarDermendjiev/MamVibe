namespace MomVibe.Application.DTOs.Users;

/// <summary>Payload for toggling holiday mode on the authenticated user's account.</summary>
public class SetHolidayModeDto
{
    /// <summary>True to enable holiday mode (hide all listings); false to disable it.</summary>
    public bool IsOnHoliday { get; set; }
}
