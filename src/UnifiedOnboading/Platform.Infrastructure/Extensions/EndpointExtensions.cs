using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;


namespace Platform.Infrastructure.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Applies standard BFF configurations to any endpoint.
    /// </summary>
    public static RouteHandlerBuilder WithDefaultBffConfig(this RouteHandlerBuilder builder) =>
         builder
            .RequireRateLimiting("PerIp")               // Your rate limit policy
            .ProducesValidationProblem()               // For FluentValidation pipeline
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
}
