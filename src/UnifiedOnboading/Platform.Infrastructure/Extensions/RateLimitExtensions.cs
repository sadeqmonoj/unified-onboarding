using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
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
                // Skip OtpPerPhone - we'll handle it separately
                if (policy.Key == "OtpPerPhone")
                {
                    continue;
                }

                rateLimiterOptions.AddPolicy(policy.Key, context =>
                    RateLimitPartition.GetSlidingWindowLimiter(  // ✅ Changed to Sliding
                        GetClientIdentifier(context),             // ✅ Better identifier
                        _ => new SlidingWindowRateLimiterOptions  // ✅ Changed to Sliding
                        {
                            PermitLimit = policy.Value.PermitLimit,
                            Window = TimeSpanParser.ParseFlexible(policy.Value.Window!),
                            SegmentsPerWindow = 5,  // ✅ Smoother rate limiting
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }
                    ));
            }

            if (options.Policies!.TryGetValue("OtpPerPhone", out PolicyRateLimit? phonePolicy))
            {
                rateLimiterOptions.AddPolicy<string>("OtpPerPhone", context =>
                {
                    string? phoneNumber = ExtractPhoneNumberFromRequest(context);

                    // If can't extract phone, fallback to IP (still protect)
                    string partitionKey = !string.IsNullOrEmpty(phoneNumber)
                        ? $"phone:{phoneNumber}"
                        : $"ip:{GetClientIdentifier(context)}";

                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = phonePolicy.PermitLimit,
                            Window = TimeSpanParser.ParseFlexible(phonePolicy.Window!),
                            SegmentsPerWindow = 6,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }
                    );
                });
            }

            rateLimiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "RATE_LIMIT_EXCEEDED",
                    message = "Too many requests. Please try again later.",
                    retryAfter = retryAfter.TotalSeconds
                }, cancellationToken);
            };
        });
        return services;
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get real IP from proxy headers first
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
        {
            string ip = forwardedFor.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                return ip;
            }
        }

        if (context.Request.Headers.TryGetValue("X-Real-IP", out StringValues realIp))
        {
            string ip = realIp.ToString().Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                return ip; 
            }                
        }

        // Fallback to connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string? ExtractPhoneNumberFromRequest(HttpContext context)
    {
        // Enable buffering so we can read body multiple times
        context.Request.EnableBuffering();

        try
        {
            // Read body
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true
            );

            string body = reader.ReadToEndAsync().Result;

            // Reset position for next read
            context.Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            // Parse JSON to find phone number
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            // Try different property names (case-sensitive)
            if (root.TryGetProperty("mobileNumber", out JsonElement phoneElement))
            {
                return phoneElement.GetString();
            }

            if (root.TryGetProperty("MobileNumber", out JsonElement phoneElement2))
            {
                return phoneElement2.GetString();
            }

            if (root.TryGetProperty("phoneNumber", out JsonElement phoneElement3))
            {
                return phoneElement3.GetString();
            }
        }
        catch
        {
            // If anything fails, just return null
            // Rate limiting will fallback to IP-based
        }
        finally
        {
            // Ensure body position is reset
            try
            { context.Request.Body.Position = 0; }
            catch { }
        }

        return null;
    }
}
