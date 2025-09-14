using System.Net.Http.Json;
using AgileActors.Core.Aggregation;
using AgileActors.Core.External;
using Microsoft.Extensions.Configuration;

namespace AgileActors.Application.Services.Providers;

public class NewsProvider : IExternalProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public string Name => "news";

    public NewsProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["ExternalApis:NewsApiKey"] ?? throw new InvalidOperationException("NewsApiKey missing");
    }

    public async Task<IReadOnlyList<AggregatedItem>> FetchAsync(AggregateQuery query, CancellationToken ct)
    {
        try
        {
            
            var q = string.IsNullOrWhiteSpace(query.Query) ? "agileactors" : query.Query!;

         
            var from = (query.From ?? DateTimeOffset.UtcNow.AddDays(-1))
                .UtcDateTime
                .ToString("yyyy-MM-dd");

       
            var validSortOptions = new[] { "relevancy", "popularity", "publishedAt" };
            var sort = string.IsNullOrWhiteSpace(query.SortBy) ||
                       !validSortOptions.Contains(query.SortBy, StringComparer.OrdinalIgnoreCase)
                ? "publishedAt"
                : query.SortBy!;

          
            var url =
                $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(q)}&from={from}&sortBy={sort}&language=en&apiKey={_apiKey}";

            using var resp = await _http.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"[NewsProvider] API returned {resp.StatusCode}: {errorBody}");
                return Array.Empty<AggregatedItem>();
            }

            var payload = await resp.Content.ReadFromJsonAsync<NewsApiResponse>(cancellationToken: ct);

            if (payload?.status == "error")
            {
                Console.WriteLine($"[NewsProvider] API error: {payload.message}");
                return Array.Empty<AggregatedItem>();
            }

            var items = payload?.articles?.Select(a => new AggregatedItem(
                Source: Name,
                Title: a.title ?? "(no title)",
                Subtitle: a.source?.name,
                Url: a.url,
                Date: a.publishedAt,
                Category: "news",
                Description: a.description,
                Raw: a
            )).ToList() ?? new List<AggregatedItem>();

            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NewsProvider] Exception: {ex.Message}");
            return Array.Empty<AggregatedItem>();
        }
    }

    private class NewsApiResponse
    {
        public string? status { get; set; }
        public int? totalResults { get; set; }

        public string? message { get; set; }  

        public List<Article>? articles { get; set; }

        public class Article
        {
            public Source? source { get; set; }
            public string? title { get; set; }
            public string? description { get; set; }
            public string? url { get; set; }
            public DateTimeOffset? publishedAt { get; set; }
        }

        public class Source { public string? name { get; set; } }
    }
}
