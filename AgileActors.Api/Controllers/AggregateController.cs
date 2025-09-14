using Microsoft.AspNetCore.Mvc;
using AgileActors.Application.Services;
using AgileActors.Core.Aggregation;
using Microsoft.AspNetCore.Authorization;

namespace AgileActors.Api.Controllers;

[ApiController]
[Route("api/aggregate")]
public class AggregateController : ControllerBase
{
    private readonly IAggregationService _svc;
    public AggregateController(IAggregationService svc) => _svc = svc;

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<AggregatedResponse>> GetPublic(
        [FromQuery] string? q,
        [FromQuery] string? category,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? sortBy,
        CancellationToken ct)
    {
        var query = new AggregateQuery(q, category, from, to, sortBy);
        var result = await _svc.AggregateAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("secure")]
    [Authorize]
    public async Task<ActionResult<AggregatedResponse>> GetSecure(
       [FromQuery] string? q,
       [FromQuery] string? category,
       [FromQuery] DateTimeOffset? from,
       [FromQuery] DateTimeOffset? to,
       [FromQuery] string? sortBy,
       CancellationToken ct)
    {
        var query = new AggregateQuery(q, category, from, to, sortBy);
        var result = await _svc.AggregateAsync(query, ct);
        return Ok(result);
    }
}
