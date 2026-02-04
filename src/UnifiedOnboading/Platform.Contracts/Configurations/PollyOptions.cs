namespace Platform.Contracts.Configurations;

public class PollyOptions
{
    public RetryPolicyOptions? Retry { get; set; }
    public CircuitBreakerOptions? CircuitBreaker { get; set; }
}
public class RetryPolicyOptions
{
    public int RetryCount { get; set; }
    public int BaseDelaySeconds { get; set; }
}
public class CircuitBreakerOptions
{
    public int AllowedBeforeBreaking { get; set; }
    public int BreakDurationSeconds { get; set; }
}
