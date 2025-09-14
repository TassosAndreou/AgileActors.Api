namespace AgileActors.Core.Aggregation;

public record AggregatedItem(
    string Source,
    string Title,
    string? Subtitle,
    string? Url,
    DateTimeOffset? Date,
    string? Category,
    string? Description,
    object? Raw
);

public record AggregatedResponse(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<AggregatedItem> Items
);

public record AggregateQuery(
    string? Query,
    string? Category,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? SortBy
);
