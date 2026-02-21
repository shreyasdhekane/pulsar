using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulsar.API.Models;
using Pulsar.API.Data;
using Pulsar.API.DTOs;

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
    [HttpPost("custom")]
    public async Task<IActionResult> AddCustomEndpoint([FromBody] AddEndpointDto dto)
    {
        if (!dto.Url.StartsWith("https://") && !dto.Url.StartsWith("http://"))
            return BadRequest("URL must start with http:// or https://");

        if (dto.Url.Contains("localhost") || dto.Url.Contains("192.168") || dto.Url.Contains("127.0.0.1"))
            return BadRequest("Private URLs are not allowed");
            
        var endpoint = new MonitoredEndpoint
        {
            Name = dto.Name,
            Url = dto.Url,
            IsFeatured = false,
            IsPublic = true,
            IntervalSeconds = 60
        };
        _db.MonitoredEndpoints.Add(endpoint);
        await _db.SaveChangesAsync();
        return Ok(endpoint);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEndpointDetail(int id)
    {
        var endpoint = await _db.MonitoredEndpoints
            .Where(e => e.Id == id)
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
                AvgResponseTime = e.PingResults.Any()
                    ? Math.Round(e.PingResults.Average(p => (double)p.ResponseTimeMs), 0)
                    : 0,
                MinResponseTime = e.PingResults.Any()
                    ? e.PingResults.Min(p => p.ResponseTimeMs)
                    : 0,
                MaxResponseTime = e.PingResults.Any()
                    ? e.PingResults.Max(p => p.ResponseTimeMs)
                    : 0,
                Last24Hours = e.PingResults
                    .Where(p => p.Timestamp >= DateTime.UtcNow.AddHours(-24))
                    .OrderBy(p => p.Timestamp)
                    .Select(p => new { p.ResponseTimeMs, p.IsUp, p.Timestamp })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (endpoint == null) return NotFound();
        return Ok(endpoint);
    }
}