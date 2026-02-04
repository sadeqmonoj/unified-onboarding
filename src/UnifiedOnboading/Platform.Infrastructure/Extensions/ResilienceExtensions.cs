using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Contracts.Configurations;
using Platform.Infrastructure.Policies;

namespace Platform.Infrastructure.Extensions;

public static class ResilienceExtensions
{
    public static IServiceCollection AddPollyResilience(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PollyOptions>(configuration.GetSection("Polly"));
        services.AddSingleton<PollyPolicies>();

        return services;
    }
}
