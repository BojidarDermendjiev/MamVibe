namespace MomVibe.Application.Interfaces;

using DTOs.SavedSearches;

public interface ISavedSearchNotifier
{
    Task NotifyAsync(string userId, SavedSearchMatchNotification notification);
}
