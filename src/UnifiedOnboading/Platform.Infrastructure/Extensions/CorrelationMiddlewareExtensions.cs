using Microsoft.AspNetCore.Builder;
using Platform.Infrastructure.Middlewares;

namespace Platform.Infrastructure.Extensions;

public static class CorrelationMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
}
