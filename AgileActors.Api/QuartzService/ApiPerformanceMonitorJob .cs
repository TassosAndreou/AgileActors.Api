using AgileActors.Core.Stats;
using Microsoft.Extensions.Logging;
using Quartz;


namespace AgileActors.Api.QuartzService
{
    public class ApiPerformanceMonitorJob : IJob
    {
        private readonly IApiStatsStore _stats;
        private readonly ILogger<ApiPerformanceMonitorJob> _logger;

        public ApiPerformanceMonitorJob(IApiStatsStore stats, ILogger<ApiPerformanceMonitorJob> logger)
        {
            _stats = stats;
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var snapshot = _stats.Snapshot();

            foreach (var api in snapshot)
            {
                if (api.TotalRequests < 5) continue; // skip low activity

                var overallAvg = api.AverageMs;
                var last5MinAvg = api.AverageLast(TimeSpan.FromMinutes(5));

                _logger.LogInformation(
                    "API {Api}: Overall Avg {Overall} ms, Last 5 Min Avg {Last5} ms",
                    api.ApiName, overallAvg, last5MinAvg
                );

                if (last5MinAvg > overallAvg * 1.5)
                {
                    _logger.LogWarning(
                        "Performance anomaly detected for {Api}: last 5 min avg {Last5} ms vs overall {Overall} ms",
                        api.ApiName, last5MinAvg, overallAvg
                    );
                }
            }

            return Task.CompletedTask;
        }
    }
}
