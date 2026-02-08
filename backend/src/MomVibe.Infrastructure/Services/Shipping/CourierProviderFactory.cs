namespace MomVibe.Infrastructure.Services.Shipping;

using Domain.Enums;
using Application.Interfaces;

/// <summary>
/// Factory that resolves the correct <see cref="ICourierProvider"/> implementation
/// based on the <see cref="CourierProvider"/> enum value.
/// Injects all registered ICourierProvider implementations and looks up by ProviderType.
/// To add a new courier: register a new ICourierProvider in DI — no factory changes needed.
/// </summary>
public class CourierProviderFactory
{
    private readonly Dictionary<CourierProvider, ICourierProvider> _providers;

    public CourierProviderFactory(IEnumerable<ICourierProvider> providers)
    {
        this._providers = providers.ToDictionary(p => p.ProviderType);
    }

    /// <summary>
    /// Gets the courier provider for the specified enum value.
    /// </summary>
    /// <param name="provider">The courier provider enum.</param>
    /// <returns>The matching <see cref="ICourierProvider"/> implementation.</returns>
    /// <exception cref="ArgumentException">Thrown when no provider is registered for the given enum value.</exception>
    public ICourierProvider GetProvider(CourierProvider provider)
    {
        if (this._providers.TryGetValue(provider, out var courierProvider))
        {
            return courierProvider;
        }

        throw new ArgumentException($"No courier provider registered for {provider}.");
    }
}
