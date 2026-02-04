using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Platform.Infrastructure;

public class CorrelationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CorrelationDelegatingHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpContext? ctx = _httpContextAccessor.HttpContext;
        if (ctx != null && ctx.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues cid))
        {
            request.Headers.TryAddWithoutValidation("X-Correlation-ID", cid.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}

