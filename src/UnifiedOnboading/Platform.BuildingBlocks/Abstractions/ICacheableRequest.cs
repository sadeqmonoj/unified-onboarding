namespace Platform.BuildingBlocks.Abstractions;

public interface ICacheableRequest
{
    string CacheKey { get; }

}
