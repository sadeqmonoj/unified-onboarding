using Microsoft.Extensions.Options;
using Platform.Contracts.Configurations;
using Polly;
using Polly.Extensions.Http;

namespace Platform.Infrastructure.Policies;

public class PollyPolicies
{
    private readonly PollyOptions _options;
    public PollyPolicies(IOptions<PollyOptions> options) => _options = options.Value;


    // Retry policy for transient HTTP errors (ex: 5xx, network failures)
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        IAsyncPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: _options!.Retry!.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(_options.Retry.BaseDelaySeconds, retryAttempt))
            );

        return retryPolicy;
    }

    // Circuit breaker to stop hammering an unhealthy downstream service
    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        IAsyncPolicy<HttpResponseMessage> circuitPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: _options!.CircuitBreaker!.AllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(_options.CircuitBreaker.BreakDurationSeconds)
            );

        return circuitPolicy;
    }
}
