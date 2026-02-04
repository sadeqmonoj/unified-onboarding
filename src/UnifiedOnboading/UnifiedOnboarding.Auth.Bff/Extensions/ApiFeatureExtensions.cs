using UnifiedOnboarding.Auth.Features.Me;

namespace UnifiedOnboarding.Auth.Bff.Extensions;

public static class ApiFeatureExtensions
{
    public static IServiceCollection AddApiFeatures(
        this IServiceCollection services)
    {

        services.AddAuthFeature();
        return services;
    }

    public static IEndpointRouteBuilder MapApiFeatures(this IEndpointRouteBuilder endpoints)
    {

        endpoints.MapAuthFeature();
        return endpoints;
    }
}
