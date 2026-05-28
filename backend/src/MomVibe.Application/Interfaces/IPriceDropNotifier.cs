namespace MomVibe.Application.Interfaces;

using DTOs.Items;

public interface IPriceDropNotifier
{
    Task NotifyAsync(string userId, PriceDropNotification notification);
}
