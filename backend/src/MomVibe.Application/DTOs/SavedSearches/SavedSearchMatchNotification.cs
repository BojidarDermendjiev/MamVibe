namespace MomVibe.Application.DTOs.SavedSearches;

public class SavedSearchMatchNotification
{
    public Guid SavedSearchId { get; set; }
    public required string SavedSearchName { get; set; }
    public required SavedSearchMatchItemDto Item { get; set; }
}
