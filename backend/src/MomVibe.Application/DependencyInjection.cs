namespace MomVibe.Application;

using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configures application-layer services: registers AutoMapper profiles, FluentValidation validators,
/// and MediatR (domain-event dispatch + INotificationHandler discovery) from the Application assembly.
/// Exposes <see cref="AddApplicationServices"/> to wire them into the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers AutoMapper profiles, FluentValidation validators, and MediatR notification handlers
    /// discovered in the Application and Infrastructure assemblies. The Infrastructure assembly is
    /// included so that <see cref="MediatR.INotificationHandler{TNotification}"/> implementations
    /// (which depend on EF Core, Stripe, etc.) are picked up alongside the events they handle.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(applicationAssembly);
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Load the Infrastructure assembly explicitly: this project has no compile-time reference
        // to Infrastructure (Clean Architecture), so handlers there are only discoverable if we
        // force the assembly to load. The runtime cost is a single Type.GetType lookup at startup.
        var infrastructureAssembly = LoadInfrastructureAssemblyIfPresent();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            if (infrastructureAssembly is not null)
                cfg.RegisterServicesFromAssembly(infrastructureAssembly);
        });

        return services;
    }

    private static Assembly? LoadInfrastructureAssemblyIfPresent()
    {
        try
        {
            return Assembly.Load("MomVibe.Infrastructure");
        }
        catch
        {
            // Tests that exercise the Application layer in isolation don't have Infrastructure
            // on the probe path; MediatR will simply not see any of its handlers, which is fine.
            return null;
        }
    }
}
