namespace AgileActors.Core.Stats;

public record ApiStatsSnapshot(
    string ApiName,
    long TotalRequests,
    double AverageMs,
    long FastCount,
    long AverageCount,
    long SlowCount
);

public interface IApiStatsStore
{
    void Record(string apiName, TimeSpan elapsed);
    ApiStatsSnapshot[] Snapshot();
}
