namespace Platform.Contracts.Configurations;

public class RateLimitOptions
{
    public GlobalRateLimit? Global { get; set; }
    public Dictionary<string, PolicyRateLimit>? Policies { get; set; }
}
public class GlobalRateLimit
{
    public int PermitLimit { get; set; }
    public string? Window { get; set; }
    public int QueueLimit { get; set; } = 0;
    public string QueueProcessingOrder { get; set; } = "OldestFirst";
}
public class PolicyRateLimit
{
    public int PermitLimit { get; set; }
    public string? Window { get; set; }
}
