namespace AgileActors.Core.Stats;

public sealed record ApiRequestSample(DateTimeOffset Timestamp, long DurationMs);

public sealed record ApiStatsSnapshot(
    string ApiName,
    long TotalRequests,
    double AverageMs,
    long FastCount,
    long AverageCount,
    long SlowCount,
    List<ApiRequestSample> History // 👈 now uses ApiRequestSample for JSON-friendly serialization
)
{
    /// <summary>
    /// Computes average latency for the given time window.
    /// </summary>
    public double AverageLast(TimeSpan window)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(window);
        var recent = History.Where(h => h.Timestamp >= cutoff).Select(h => h.DurationMs).ToList();
        return recent.Any() ? recent.Average() : 0;
    }

    /// <summary>
    /// Detects if the recent average is significantly worse than the overall average.
    /// </summary>
    public bool HasAnomaly(TimeSpan window, double thresholdMultiplier = 1.5)
    {
        var recent = AverageLast(window);
        return recent > 0 && recent > AverageMs * thresholdMultiplier;
    }
}

public interface IApiStatsStore
{
    void Record(string apiName, TimeSpan elapsed);
    ApiStatsSnapshot[] Snapshot();
}
