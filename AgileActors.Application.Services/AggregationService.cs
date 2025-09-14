using Microsoft.Extensions.Caching.Memory;
using AgileActors.Core.Aggregation;
using AgileActors.Core.External;
using AgileActors.Shared.Caching;
using AgileActors.Core.Stats;

namespace AgileActors.Application.Services;

public interface IAggregationService
{
    Task<AggregatedResponse> AggregateAsync(AggregateQuery query, CancellationToken ct);
}

public class AggregationService : IAggregationService
{
    private readonly IEnumerable<IExternalProvider> _providers;
    private readonly IMemoryCache _cache;
    private readonly IApiStatsStore _stats;

    public AggregationService(IEnumerable<IExternalProvider> providers, IMemoryCache cache, IApiStatsStore stats)
    {
        _providers = providers;
        _cache = cache;
        _stats = stats;
    }

    public async Task<AggregatedResponse> AggregateAsync(AggregateQuery query, CancellationToken ct)
    {
        var key = CacheKeys.Aggregation(query);
        if (_cache.TryGetValue(key, out AggregatedResponse cached))
            return cached;

        IEnumerable<IExternalProvider> providers;

        if (string.IsNullOrWhiteSpace(query.Category))
        {
            providers = _providers.Where(p =>
                p.Name.Equals("news", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("spotify", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            var category = query.Category.ToLowerInvariant();
            providers = _providers.Where(p => p.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        var tasks = providers.Select(p => FetchSafe(p, query, ct)).ToArray();
        var results = await Task.WhenAll(tasks);
        var items = results.SelectMany(x => x).ToList();

        var snapshot = _stats.Snapshot();
        foreach (var api in snapshot)
        {
            Console.WriteLine($"{api.ApiName} - Total: {api.TotalRequests}, Avg: {api.AverageMs} ms");
        }

        items = ApplySort(items, query.SortBy);

        var response = new AggregatedResponse(DateTimeOffset.UtcNow, items);
        _cache.Set(key, response, TimeSpan.FromSeconds(30));
        return response;
    }

    private static List<AggregatedItem> ApplySort(List<AggregatedItem> items, string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return items;

        return sortBy.ToLowerInvariant() switch
        {
            "date" => items.OrderByDescending(i => i.Date ?? DateTimeOffset.MinValue).ToList(),
            "source" => items.OrderBy(i => i.Source).ToList(),
            "title" => items.OrderBy(i => i.Title).ToList(),
            _ => items // unknown sortBy → leave as-is
        };
    }

    private static async Task<IReadOnlyList<AggregatedItem>> FetchSafe(IExternalProvider p, AggregateQuery q, CancellationToken ct)
    {
        try { return await p.FetchAsync(q, ct); }
        catch { return Array.Empty<AggregatedItem>(); }
    }
}
