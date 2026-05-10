namespace MomVibe.Application;

using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configures application-layer services: registers AutoMapper profiles and FluentValidation validators
/// from the executing assembly, exposing AddApplicationServices to wire them into the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers AutoMapper profiles and FluentValidation validators from the Application assembly.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}
