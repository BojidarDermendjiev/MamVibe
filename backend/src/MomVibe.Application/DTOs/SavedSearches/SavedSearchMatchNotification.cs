namespace MomVibe.Application.DTOs.SavedSearches;

using Items;

public class SavedSearchMatchNotification
{
    public Guid SavedSearchId { get; set; }
    public required string SavedSearchName { get; set; }
    public required ItemDto Item { get; set; }
}
