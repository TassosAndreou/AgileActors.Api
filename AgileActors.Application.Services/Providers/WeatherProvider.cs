using System.Net.Http.Json;
using AgileActors.Core.Aggregation;
using AgileActors.Core.External;
using AgileActors.Shared;
using Microsoft.Extensions.Configuration;

namespace AgileActors.Application.Services.Providers;

public sealed class WeatherProvider : IExternalProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public string Name => "weather";

    public WeatherProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["ExternalApis:OpenWeatherApiKey"] ?? throw new InvalidOperationException("OpenWeatherApiKey missing");
    }

    public async Task<IReadOnlyList<AggregatedItem>> FetchAsync(AggregateQuery query, CancellationToken ct)
    {
        try
        {
            var lat = double.TryParse(Environment.GetEnvironmentVariable("OWM_LAT"), out var l) ? l : 37.98;
            var lon = double.TryParse(Environment.GetEnvironmentVariable("OWM_LON"), out var g) ? g : 23.72;

            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";

            using var resp = await _http.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync(ct);
                Console.WriteLine($"[WeatherProvider] API returned {resp.StatusCode}: {errorBody}");
                return Array.Empty<AggregatedItem>();
            }

            var payload = await resp.Content.ReadFromJsonAsync<WeatherResponse>(cancellationToken: ct);
            var items = new List<AggregatedItem>();

            if (payload is not null)
            {
                items.Add(new AggregatedItem(
                    Source: Name,
                    Title: $"Current {payload.main.temp}°C (feels {payload.main.feels_like}°C)",
                    Subtitle: $"Humidity {payload.main.humidity}%, Wind {payload.wind.speed} m/s",
                    Url: null,
                    Date: DateTimeOffset.FromUnixTimeSeconds(payload.dt),
                    Category: "weather",
                    Description: payload.weather?.FirstOrDefault()?.description,
                    Raw: payload
                ));
            }

            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WeatherProvider] Exception: {ex.Message}");
            return Array.Empty<AggregatedItem>();
        }
    }



    private sealed class OneCallResponse
    {
        public Current? current { get; set; }
        public List<Hourly>? hourly { get; set; }

        public sealed class Weather { public string? description { get; set; } }

        public class Current
        {
            public long dt { get; set; }
            public double temp { get; set; }
            public double feels_like { get; set; }
            public int humidity { get; set; }
            public double wind_speed { get; set; }
            public List<Weather>? weather { get; set; }
        }

        public sealed class Hourly : Current { }
    }
}
