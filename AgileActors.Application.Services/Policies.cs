using System.Diagnostics;
using AgileActors.Core.Stats;
using Microsoft.Extensions.Http.Resilience;

namespace AgileActors.Application.Services;

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
            Console.WriteLine($"[{_apiName}] Took {sw.ElapsedMilliseconds} ms");
            _store.Record(_apiName, sw.Elapsed);
        }
    }
}
