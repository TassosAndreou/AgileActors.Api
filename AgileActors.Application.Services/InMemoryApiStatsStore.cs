using System.Collections.Concurrent;

namespace AgileActors.Core.Stats;

public class InMemoryApiStatsStore : IApiStatsStore
{
    private readonly ConcurrentDictionary<string, List<ApiRequestSample>> _history = new();

    public void Record(string api, TimeSpan elapsed)
    {
        var entry = new ApiRequestSample(DateTimeOffset.UtcNow, (long)elapsed.TotalMilliseconds);
        var list = _history.GetOrAdd(api, _ => new List<ApiRequestSample>());

        lock (list)
        {
            list.Add(entry);

            // Keep only last 1 hour of history
            var cutoff = DateTimeOffset.UtcNow.AddHours(-1);
            list.RemoveAll(x => x.Timestamp < cutoff);
        }
    }

    public ApiStatsSnapshot[] Snapshot()
    {
        return _history.Select(kvp =>
        {
            var list = kvp.Value.ToList();
            var avg = list.Any() ? list.Average(x => (double)x.DurationMs) : 0;

            return new ApiStatsSnapshot(
                ApiName: kvp.Key,
                TotalRequests: list.Count,
                AverageMs: avg,
                FastCount: list.Count(x => x.DurationMs < 100),
                AverageCount: list.Count(x => x.DurationMs is >= 100 and < 200),
                SlowCount: list.Count(x => x.DurationMs >= 200),
                History: list
            );
        }).ToArray();
    }
}
