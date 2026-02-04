using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Platform.BuildingBlocks.Abstractions;
using Platform.BuildingBlocks.CustomMediator;

namespace Platform.Infrastructure.CustomMediator.Behaviors;

public class CacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(5);
    public CacheBehavior(IMemoryCache cache, ILogger<CacheBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (request is not ICacheableRequest cacheableRequest)
        {
            return await next();
        }

        string cacheKey = cacheableRequest.CacheKey; //GenerateCacheKey(request);


        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse))
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return cachedResponse!;
        }

        _logger.LogInformation("Cache miss for {CacheKey}, invoking handler", cacheKey);
        TResponse response = await next().ConfigureAwait(false);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaultCacheDuration
        };
        _cache.Set<TResponse>(cacheKey, response, options);
        _logger.LogInformation("Cached response for {CacheKey} for {Duration} seconds", cacheKey, _defaultCacheDuration.TotalSeconds);
        return response;
    }

}
