using Microsoft.AspNetCore.Mvc;
using AgileActors.Core.Stats;
using Microsoft.AspNetCore.Authorization;

namespace AgileActors.Api.Controllers;

[ApiController]
[Route("api/stats")]
[AllowAnonymous]
public class StatsController : ControllerBase
{
    private readonly IApiStatsStore _stats;
    public StatsController(IApiStatsStore stats) => _stats = stats;

    [HttpGet]
    public ActionResult<ApiStatsSnapshot[]> Get() => _stats.Snapshot();
}
