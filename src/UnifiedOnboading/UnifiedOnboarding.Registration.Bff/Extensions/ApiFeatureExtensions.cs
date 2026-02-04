using UnifiedOnboarding.Registration.Features.OTP;

namespace UnifiedOnboarding.Registration.Bff.Extensions;

public static class ApiFeatureExtensions
{
    public static IServiceCollection AddApiFeatures(
        this IServiceCollection services)
    {
        services.AddOtpFeatures();
        return services;
    }

    public static IEndpointRouteBuilder MapApiFeatures(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOtpFeature();
        return endpoints;
    }
}
