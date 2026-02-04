using Microsoft.AspNetCore.Http;

namespace Platform.BuildingBlocks.Results;


public sealed record Error
{
    public string Code { get; init; }
    public string Message { get; init; }
    public ErrorType Type { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }

    private Error(ErrorType type, string code, string message, Dictionary<string, string[]>? validationErrors = null)
    {
        Type = type;
        Code = code;
        Message = message;
        ValidationErrors = validationErrors;
    }

    // Factory methods
    public static Error Validation(string code, string message, Dictionary<string, string[]>? errors = null)
        => new(ErrorType.Validation, code, message, errors);

    public static Error NotFound(string code, string message)
        => new(ErrorType.NotFound, code, message);

    public static Error Unauthorized(string code, string message)
        => new(ErrorType.Unauthorized, code, message);

    public static Error Server(string code, string message)
        => new(ErrorType.Server, code, message);

    public static Error RateLimit(string message)
        => new(ErrorType.RateLimit, "RATE_LIMIT_EXCEEDED", message);

    // Predefined errors
    public static Error OtpExpired()
        => Validation("OTP_EXPIRED", "The OTP has expired");

    public static Error OtpInvalid()
        => Validation("OTP_INVALID", "The OTP is invalid");

    public static Error ServiceUnavailable()
        => Server("SERVICE_UNAVAILABLE", "Downstream service is temporarily unavailable");
}

public enum ErrorType
{
    Validation,
    NotFound,
    Unauthorized,
    Server,
    RateLimit
}
