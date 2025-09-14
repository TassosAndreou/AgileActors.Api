using Microsoft.AspNetCore.Mvc;
using AgileActors.Core.Stats;
using Microsoft.AspNetCore.Authorization;

namespace AgileActors.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly IApiStatsStore _stats;
    public StatsController(IApiStatsStore stats) => _stats = stats;


    [HttpGet("public")]
    [AllowAnonymous]
    public ActionResult<ApiStatsSnapshot[]> GetPublic()
    {
        return _stats.Snapshot();
    }

    [HttpGet("secure")]
    [Authorize]
    public ActionResult<ApiStatsSnapshot[]> GetSecure()
    {
        return _stats.Snapshot();
    }
}
