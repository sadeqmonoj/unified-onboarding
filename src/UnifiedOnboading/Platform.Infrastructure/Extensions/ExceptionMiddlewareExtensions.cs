using Microsoft.AspNetCore.Builder;
using Platform.Infrastructure.Middlewares;

namespace Platform.Infrastructure.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}
