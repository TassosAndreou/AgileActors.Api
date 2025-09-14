//using System.Net.Http.Headers;
//using AgileActors.Application.Services;
//using AgileActors.Application.Services.Providers;
//using AgileActors.Core.External;
//using AgileActors.Core.Stats;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Http.Resilience;


//namespace AgileActors.Api;

//public static class HttpClientRegistration
//{
//    public static IServiceCollection AddExternalProviders(this IServiceCollection services)
//    {
//        services.AddHttpClient<NewsProvider>(c =>
//        {
//            c.BaseAddress = new Uri("https://newsapi.org/");
//        })
//     .AddResilienceHandler("news-pipeline", pipeline =>
//     {
//         pipeline.AddRetryPolicy(new HttpRetryStrategyOptions
//         {
//             MaxRetryAttempts = 3,
//             BackoffType = DelayBackoffType.Exponential, // nice exponential backoff
//             Delay = TimeSpan.FromMilliseconds(200)      // base delay
//         });

//         pipeline.AddTimeout(TimeSpan.FromSeconds(10));
//     })
//     .AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "news"));
//        // Weather
//        services.AddHttpClient<WeatherProvider>(c =>
//        {
//            c.BaseAddress = new Uri("https://api.openweathermap.org/");
//        })
//        .AddResilienceHandler("weather-pipeline", pipeline =>
//        {
//            pipeline.AddRetry(new HttpRetryStrategyOptions { MaxRetryAttempts = 3 });
//            pipeline.AddTimeout(TimeSpan.FromSeconds(10));
//        })
//        .AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "weather"));

//        // Spotify
//        services.AddHttpClient<SpotifyProvider>(c =>
//        {
//            c.BaseAddress = new Uri("https://api.spotify.com/");
//            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//        })
//        .AddResilienceHandler("spotify-pipeline", pipeline =>
//        {
//            pipeline.AddRetry(new HttpRetryStrategyOptions { MaxRetryAttempts = 3 });
//            pipeline.AddTimeout(TimeSpan.FromSeconds(10));
//        })
//        .AddHttpMessageHandler(sp => new TimingHandler(sp.GetRequiredService<IApiStatsStore>(), "spotify"));

//        // Register providers for DI
//        services.AddScoped<IExternalProvider, NewsProvider>();
//        services.AddScoped<IExternalProvider, WeatherProvider>();
//        services.AddScoped<IExternalProvider, SpotifyProvider>();

//        return services;
//    }
//}
