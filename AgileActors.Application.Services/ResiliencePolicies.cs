using System.Diagnostics;
using AgileActors.Core.Stats;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace AgileActors.Application.Services;

// Centralized configuration for resilience pipelines
public static class ResiliencePolicies
{
    public static void ConfigureStandardResilience(ResiliencePipelineBuilder<HttpResponseMessage> pipeline)
    {
        // Retry with exponential backoff
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(200),
            UseJitter = true
        });

        // Circuit breaker: break if >50% failures in 30s window
        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(20)
        });

        // Timeout safeguard
        pipeline.AddTimeout(TimeSpan.FromSeconds(10));
    }
}

// Keeps working exactly as before
public sealed class TimingHandler : DelegatingHandler
{
    private readonly IApiStatsStore _store;
    private readonly string _apiName;

    public TimingHandler(IApiStatsStore store, string apiName) => (_store, _apiName) = (store, apiName);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await base.SendAsync(request, ct);
        }
        finally
        {
            sw.Stop();
            _store.Record(_apiName, sw.Elapsed);
        }
    }
}
