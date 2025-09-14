using System.Net.Http.Headers;
using System.Net.Http.Json;
using AgileActors.Core.Aggregation;
using AgileActors.Core.External;
using Microsoft.Extensions.Configuration;

namespace AgileActors.Application.Services.Providers;

public sealed class SpotifyProvider : IExternalProvider
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public string Name => "spotify";

    public SpotifyProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<IReadOnlyList<AggregatedItem>> FetchAsync(AggregateQuery query, CancellationToken ct)
    {
        try
        {
            var clientId = _config["ExternalApis:Spotify:ClientId"]
                           ?? throw new InvalidOperationException("Spotify ClientId missing");
            var clientSecret = _config["ExternalApis:Spotify:ClientSecret"]
                               ?? throw new InvalidOperationException("Spotify ClientSecret missing");

            var token = await GetTokenAsync(clientId, clientSecret, ct);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var q = query.Query!;
            var url = $"v1/search?type=track&limit=5&q={Uri.EscapeDataString(q)}";

            using var resp = await _http.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"[SpotifyProvider] API returned {resp.StatusCode}: {errorBody}");
                return Array.Empty<AggregatedItem>();
            }

            var payload = await resp.Content.ReadFromJsonAsync<SpotifySearchResponse>(cancellationToken: ct);

            var items = payload?.tracks?.items?.Select(t => new AggregatedItem(
                Source: Name,
                Title: t.name ?? "(unknown)",
                Subtitle: t.artists is { Count: > 0 }
                    ? string.Join(", ", t.artists.Select(a => a.name))
                    : null,
                Url: t.external_urls?["spotify"],
                Date: null,
                Category: "music",
                Description: t.album?.name,
                Raw: t
            )).ToList() ?? new List<AggregatedItem>();

            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SpotifyProvider] Exception: {ex.Message}");
            return Array.Empty<AggregatedItem>();
        }
    }

    private async Task<string> GetTokenAsync(string clientId, string clientSecret, CancellationToken ct)
    {
        using var tokenReq = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            })
        };

        using var resp = await _http.SendAsync(tokenReq, ct);
        resp.EnsureSuccessStatusCode();

        var tok = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        return tok?.access_token ?? throw new InvalidOperationException("Spotify token missing");
    }

    private sealed class TokenResponse { public string? access_token { get; set; } }

    private sealed class SpotifySearchResponse
    {
        public Tracks? tracks { get; set; }
        public sealed class Tracks
        {
            public List<Track>? items { get; set; }
        }
        public sealed class Track
        {
            public string? name { get; set; }
            public Album? album { get; set; }
            public List<Artist>? artists { get; set; }
            public Dictionary<string, string>? external_urls { get; set; }
        }
        public sealed class Album { public string? name { get; set; } }
        public sealed class Artist { public string? name { get; set; } }
    }
}
