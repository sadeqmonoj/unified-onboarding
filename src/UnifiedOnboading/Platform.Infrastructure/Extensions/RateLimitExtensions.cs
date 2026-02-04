using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Contracts.Configurations;
using Platform.Infrastructure.Utilities;

namespace Platform.Infrastructure.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind config
        var options = new RateLimitOptions();
        configuration.GetSection("RateLimit").Bind(options);
        services.AddRateLimiter(rateLimiterOptions =>
        {
            // Global limit
            rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                 RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: "global",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options!.Global!.PermitLimit,
                        Window = TimeSpanParser.ParseFlexible(options!.Global!.Window!),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = options.Global.QueueLimit
                    })
            );
            // Additional policies from config
            foreach (KeyValuePair<string, PolicyRateLimit> policy in options!.Policies!)
            {
                rateLimiterOptions.AddPolicy(policy.Key, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = policy.Value.PermitLimit,
                            Window = TimeSpanParser.ParseFlexible(policy!.Value!.Window!),
                            QueueLimit = 0
                        }
                    ));
            }
        });
        return services;
    }
}
