using System.Reflection;
using Platform.Contracts.Configurations;
using Platform.Infrastructure;
using Platform.Infrastructure.Extensions;
using Platform.Infrastructure.Middlewares;
using Platform.Infrastructure.Policies;
using UnifiedOnboarding.Registration.Features.OTP.Abstractions;
using UnifiedOnboarding.Registration.Features.OTP.MockHttpMessageHandlers;
using UnifiedOnboarding.Registration.Features.OTP.Services;

namespace UnifiedOnboarding.Registration.Bff.Extensions;

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

#if DEBUG
        services.AddHttpClient<IOtpApi, OtpApi>()
           .ConfigurePrimaryHttpMessageHandler(() => new FakeOtpHttpMessageHandler())
           .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://fake-otp-service"));
#else
        string otpBaseUrl = configuration["Services:Otp"] ?? "http://otp";
        services.AddHttpClient<IOtpApi, OtpApi>("OtpApiClient", client =>
            client.BaseAddress = new Uri(otpBaseUrl))
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(90),
            })
            .AddPolicyHandler((sp, _) => sp.GetRequiredService<PollyPolicies>().GetRetryPolicy())
            .AddPolicyHandler((sp, _) => sp.GetRequiredService<PollyPolicies>().GetCircuitBreakerPolicy())
            .AddHttpMessageHandler<CorrelationDelegatingHandler>();
#endif


        string authBaseUrl = configuration["Services:Auth"] ?? "http://auth";
        services.AddHttpClient<IOtpTokenApi, OtpTokenApi>("AuthTokenClient", client =>
            client.BaseAddress = new Uri(authBaseUrl))
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(90),
            })
            .AddPolicyHandler((sp, _) => sp.GetRequiredService<PollyPolicies>().GetRetryPolicy())
            .AddPolicyHandler((sp, _) => sp.GetRequiredService<PollyPolicies>().GetCircuitBreakerPolicy())
            .AddHttpMessageHandler<CorrelationDelegatingHandler>();

        //
        services.AddTransient<CorrelationDelegatingHandler>();
        services.AddSingleton<CorrelationIdMiddleware>();
        services.AddSingleton<ExceptionMiddleware>();

        services.Configure<OtpAuthOptions>(configuration.GetSection("OtpAuthCredential"));

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
