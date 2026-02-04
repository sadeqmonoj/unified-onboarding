using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Platform.Infrastructure.Middlewares;

public class CorrelationIdMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues cid) || string.IsNullOrWhiteSpace(cid))
        {
            cid = System.Guid.NewGuid().ToString();
        }

        context.Response.Headers["X-Correlation-ID"] = cid.ToString();
        using (Serilog.Context.LogContext.PushProperty("correlation_id", cid.ToString()))
        {
            await next(context);
        }
    }
}
