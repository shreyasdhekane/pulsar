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
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalEndpoints = await _db.MonitoredEndpoints.CountAsync(e => e.IsFeatured);
        var totalPingsToday = await _db.PingResults
            .Where(p => p.Timestamp >= DateTime.UtcNow.AddHours(-24))
            .CountAsync();
        var upPings = await _db.PingResults
            .Where(p => p.Timestamp >= DateTime.UtcNow.AddHours(-24) && p.IsUp)
            .CountAsync();
        var avgUptime = totalPingsToday > 0
            ? Math.Round(upPings * 100.0 / totalPingsToday, 1)
            : 0;
        var avgResponseTime = await _db.PingResults
            .Where(p => p.Timestamp >= DateTime.UtcNow.AddHours(-24) && p.IsUp)
            .AverageAsync(p => (double?)p.ResponseTimeMs) ?? 0;

        return Ok(new {
            totalEndpoints,
            totalPingsToday,
            avgUptimePercent = avgUptime,
            avgResponseTimeMs = Math.Round(avgResponseTime)
        });
    }
}