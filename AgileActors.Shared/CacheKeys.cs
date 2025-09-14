using AgileActors.Core.Aggregation;

namespace AgileActors.Shared.Caching;

public static class CacheKeys
{
    public static string Aggregation(AggregateQuery q) =>
        $"agg::{q.Query}::{q.Category}::{q.From?.UtcDateTime:o}::{q.To?.UtcDateTime:o}::{q.SortBy}";
}
