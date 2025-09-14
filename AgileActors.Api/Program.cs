using System.Net.Http.Headers;
using AgileActors.Application.Services;
using AgileActors.Application.Services.Providers;
using AgileActors.Core.External;
using AgileActors.Core.Stats;
using Microsoft.Extensions.Http.Resilience;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IApiStatsStore, InMemoryApiStatsStore>();
builder.Services.AddScoped<IAggregationService, AggregationService>();

// --- HttpClients with built-in resilience ---
builder.Services.AddHttpClient<NewsProvider>(c =>
{
    c.BaseAddress = new Uri("https://newsapi.org/");
})
.AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "news"));

builder.Services.AddHttpClient<WeatherProvider>(c =>
{
    c.BaseAddress = new Uri("https://api.openweathermap.org/");
})
.AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "weather"));

builder.Services.AddHttpClient<SpotifyProvider>(c =>
{
    c.BaseAddress = new Uri("https://api.spotify.com/");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "spotify"));

// Register providers as IExternalProvider
builder.Services.AddScoped<IExternalProvider, NewsProvider>();
builder.Services.AddScoped<IExternalProvider, WeatherProvider>();
builder.Services.AddScoped<IExternalProvider, SpotifyProvider>();

// --- API pipeline ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Agile Actors API");
        options.WithTheme(ScalarTheme.BluePlanet);
        options.WithSidebar(true);

    });
}

//app.UseSwagger();
//app.UseSwaggerUI();

app.MapControllers();

app.Run();
