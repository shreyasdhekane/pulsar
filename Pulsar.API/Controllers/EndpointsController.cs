using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulsar.API.Data;

namespace Pulsar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EndpointsController : ControllerBase
{
    private readonly PulsarDbContext _db;

    public EndpointsController(PulsarDbContext db)
    {
        _db = db;
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var endpoints = await _db.MonitoredEndpoints
            .Where(e => e.IsFeatured)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Url,
                LatestPing = e.PingResults
                    .OrderByDescending(p => p.Timestamp)
                    .Select(p => new { p.StatusCode, p.ResponseTimeMs, p.IsUp, p.Timestamp })
                    .FirstOrDefault(),
                UptimePercent = e.PingResults.Any()
                    ? Math.Round(e.PingResults.Count(p => p.IsUp) * 100.0 / e.PingResults.Count(), 1)
                    : 0,
                RecentPings = e.PingResults
                    .OrderByDescending(p => p.Timestamp)
                    .Take(20)
                    .Select(p => new { p.ResponseTimeMs, p.IsUp, p.Timestamp })
                    .ToList()
            })
            .ToListAsync();

        return Ok(endpoints);
    }
}