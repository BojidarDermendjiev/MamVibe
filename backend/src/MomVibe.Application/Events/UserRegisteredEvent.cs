namespace MomVibe.Application.Events;

/// <summary>
/// Raised once a new <c>ApplicationUser</c> has been created and assigned the default role.
/// Handlers fire the n8n <c>user.registered</c> webhook; this event also exists as the
/// natural extension point for future onboarding (welcome email, analytics, etc.).
/// </summary>
/// <param name="UserId">The new user's identifier.</param>
public sealed record UserRegisteredEvent(string UserId) : IDomainEvent;
