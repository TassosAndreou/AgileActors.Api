using System.Collections.Concurrent;
using AgileActors.Core.Stats;

namespace AgileActors.Application.Services;

public class InMemoryApiStatsStore : IApiStatsStore
{
    private class Counter
    {
        public long Total;
        public double TotalMs;
        public long Fast;
        public long Average;
        public long Slow;
    }

    private readonly ConcurrentDictionary<string, Counter> _data = new();
    private readonly (double fast, double avg) _thresholds;

    public InMemoryApiStatsStore(double fastMs = 100, double avgMs = 200) =>
        _thresholds = (fastMs, avgMs);

    public void Record(string apiName, TimeSpan elapsed)
    {
        var ms = elapsed.TotalMilliseconds;
        var c = _data.GetOrAdd(apiName, _ => new Counter());

        c.Total++;
        c.TotalMs += ms;
        if (ms < _thresholds.fast) c.Fast++;
        else if (ms < _thresholds.avg) c.Average++;
        else c.Slow++;
    }

    public ApiStatsSnapshot[] Snapshot()
    {
        return _data.Select(kv =>
        {
            var c = kv.Value;
            var total = Math.Max(1, c.Total);
            return new ApiStatsSnapshot(
                kv.Key,
                c.Total,
                c.TotalMs / total,
                c.Fast,
                c.Average,
                c.Slow
            );
        }).ToArray();
    }
}
