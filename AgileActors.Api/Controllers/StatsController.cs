using Microsoft.AspNetCore.Mvc;
using AgileActors.Core.Stats;

namespace AgileActors.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly IApiStatsStore _stats;
    public StatsController(IApiStatsStore stats) => _stats = stats;

    [HttpGet]
    public ActionResult<ApiStatsSnapshot[]> Get() => _stats.Snapshot();
}
