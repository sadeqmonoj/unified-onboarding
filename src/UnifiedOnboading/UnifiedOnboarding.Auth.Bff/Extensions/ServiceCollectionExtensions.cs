using System.Reflection;
using Platform.BuildingBlocks.Abstractions;
using Platform.Infrastructure;
using Platform.Infrastructure.Extensions;
using Platform.Infrastructure.Middlewares;

namespace UnifiedOnboarding.Auth.Bff.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomRateLimiting(configuration);
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Auth Config
        services.AddOpenIddictAuthValidation(configuration);

        // Resiliency
        services.AddPollyResilience(configuration);

        //
        services.AddTransient<CorrelationDelegatingHandler>();
        services.AddSingleton<CorrelationIdMiddleware>();
        services.AddSingleton<ExceptionMiddleware>();

        services.AddScoped<ICurrentUser, CurrentUser>();

        // Mediator scanning
        Assembly[] assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => a.GetName().Name!.StartsWith("UnifiedOnboarding.") &&
                        a.GetName().Name!.Contains(".Features."))
            .ToArray();
        services.AddSimpleMediator(assemblies);

        return services;
    }
}
