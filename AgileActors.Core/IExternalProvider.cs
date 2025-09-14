using AgileActors.Core.Aggregation;

namespace AgileActors.Core.External;

public interface IExternalProvider
{
    string Name { get; }
    Task<IReadOnlyList<AggregatedItem>> FetchAsync(AggregateQuery query, CancellationToken ct);
}
