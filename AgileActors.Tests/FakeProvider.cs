using AgileActors.Application.Services;
using AgileActors.Core.Aggregation;
using AgileActors.Core.External;
using AgileActors.Core.Stats;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AgileActors.Tests
{
    public class AggregationServiceTests
    {
        private class FakeProvider : IExternalProvider
        {
            public string Name { get; }
            private readonly IReadOnlyList<AggregatedItem> _items;

            public FakeProvider(string name, IReadOnlyList<AggregatedItem> items)
            {
                Name = name;
                _items = items;
            }

            public Task<IReadOnlyList<AggregatedItem>> FetchAsync(AggregateQuery query, CancellationToken ct)
                => Task.FromResult(_items);
        }

        [Fact]
        public async Task AggregateAsync_ReturnsFilteredResults()
        {
            var providers = new IExternalProvider[]
            {
                new FakeProvider("news", new [] { new AggregatedItem("news", "title", "sub", "url", DateTimeOffset.Now, "news", "desc", null) }),
                new FakeProvider("spotify", new [] { new AggregatedItem("spotify", "song", "artist", "url", DateTimeOffset.Now, "music", "album", null) })
            };

            var cache = new MemoryCache(new MemoryCacheOptions());
            var svc = new AggregationService(providers, cache, new InMemoryApiStatsStore());

            var query = new AggregateQuery("test", "news", null, null, null);

            var result = await svc.AggregateAsync(query, default);

            Assert.Single(result.Items);
            Assert.Equal("news", result.Items.First().Source);
        }
    }
}
