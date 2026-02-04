using Microsoft.AspNetCore.Http;

namespace Platform.BuildingBlocks.Results;

public sealed record Error(
    string Code,
    string Message,
    int HttpStatusCode = StatusCodes.Status400BadRequest
)
{
    public static Error Validation(string message)
        => new("validation_error", message, StatusCodes.Status400BadRequest);

    public static Error NotFound(string message)
        => new("not_found", message, StatusCodes.Status404NotFound);

    public static Error Unauthorized(string message)
        => new("unauthorized", message, StatusCodes.Status401Unauthorized);

    public static Error Forbidden(string message)
        => new("forbidden", message, StatusCodes.Status403Forbidden);

    public static Error Conflict(string message)
        => new("conflict", message, StatusCodes.Status409Conflict);

    public static Error Server(string message)
        => new("server_error", message, StatusCodes.Status500InternalServerError);
}
