using Microsoft.Extensions.Caching.Memory;
using AgileActors.Core.Aggregation;
using AgileActors.Core.External;
using AgileActors.Shared.Caching;

namespace AgileActors.Application.Services;

public interface IAggregationService
{
    Task<AggregatedResponse> AggregateAsync(AggregateQuery query, CancellationToken ct);
}

public class AggregationService : IAggregationService
{
    private readonly IEnumerable<IExternalProvider> _providers;
    private readonly IMemoryCache _cache;

    public AggregationService(IEnumerable<IExternalProvider> providers, IMemoryCache cache)
    {
        _providers = providers;
        _cache = cache;
    }

    public async Task<AggregatedResponse> AggregateAsync(AggregateQuery query, CancellationToken ct)
    {
        var key = CacheKeys.Aggregation(query);
        if (_cache.TryGetValue(key, out AggregatedResponse cached))
            return cached;

        var tasks = _providers.Select(p => FetchSafe(p, query, ct)).ToArray();
        var results = await Task.WhenAll(tasks);
        var items = results.SelectMany(x => x).ToList();

        var response = new AggregatedResponse(DateTimeOffset.UtcNow, items);
        _cache.Set(key, response, TimeSpan.FromSeconds(30));
        return response;
    }

    private static async Task<IReadOnlyList<AggregatedItem>> FetchSafe(IExternalProvider p, AggregateQuery q, CancellationToken ct)
    {
        try { return await p.FetchAsync(q, ct); }
        catch { return Array.Empty<AggregatedItem>(); }
    }
}
