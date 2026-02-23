using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Pulsar.API.Models;
using Pulsar.API.Data;
using Pulsar.API.DTOs;
using System.Text.Json;
using System.Net.Http;

namespace Pulsar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EndpointsController : ControllerBase
{
    private readonly PulsarDbContext _db;
    private readonly IConfiguration _config;

   public EndpointsController(PulsarDbContext db, IConfiguration config)
   {
    _db = db;
    _config = config;
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

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyEndpoints()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var endpoints = await _db.MonitoredEndpoints
            .Where(e => e.UserId == userId)
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


    [HttpPost("custom")]
    [Authorize]
    public async Task<IActionResult> AddCustomEndpoint([FromBody] AddEndpointDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (!dto.Url.StartsWith("https://") && !dto.Url.StartsWith("http://"))
            return BadRequest("URL must start with http:// or https://");

        if (dto.Url.Contains("localhost") || dto.Url.Contains("192.168") || dto.Url.Contains("127.0.0.1"))
            return BadRequest("Private URLs are not allowed");

        var endpoint = new MonitoredEndpoint
        {
            Name = dto.Name,
            Url = dto.Url,
            UserId = userId,
            IsFeatured = false,
            IsPublic = false,
            IntervalSeconds = 60
        };
        _db.MonitoredEndpoints.Add(endpoint);
        await _db.SaveChangesAsync();
        return Ok(endpoint);
    }

    [HttpGet("{id}")]
public async Task<IActionResult> GetEndpoint(int id)
{
    var endpoint = await _db.MonitoredEndpoints
        .Where(e => e.Id == id)
        .Select(e => new
        {
            e.Id,
            e.Name,
            e.Url,
            e.IsFeatured,
            e.IsPublic,
            e.IntervalSeconds,
            LatestPing = e.PingResults
                .OrderByDescending(p => p.Timestamp)
                .Select(p => new { p.StatusCode, p.ResponseTimeMs, p.IsUp, p.Timestamp })
                .FirstOrDefault()
        })
        .FirstOrDefaultAsync();

    if (endpoint == null)
    {
        return NotFound();
    }

    return Ok(endpoint);
}

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteEndpoint(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var endpoint = await _db.MonitoredEndpoints
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (endpoint == null) return NotFound();

        _db.MonitoredEndpoints.Remove(endpoint);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("{id}/insights")]
public async Task<IActionResult> GetInsights(int id)
{
    var pings = await _db.PingResults
        .Where(p => p.EndpointId == id && p.Timestamp >= DateTime.UtcNow.AddHours(-24))
        .OrderBy(p => p.Timestamp)
        .ToListAsync();

    if (!pings.Any()) return Ok(new { summary = "Not enough data yet.", incidents = new List<object>(), p50 = 0, p95 = 0, p99 = 0 });

    // p50/p95/p99
    var sorted = pings.Where(p => p.IsUp).Select(p => (double)p.ResponseTimeMs).OrderBy(x => x).ToList();
    double p50 = 0, p95 = 0, p99 = 0;
    if (sorted.Any())
    {
        p50 = sorted[(int)(sorted.Count * 0.50)];
        p95 = sorted[(int)(sorted.Count * 0.95)];
        p99 = sorted[Math.Min((int)(sorted.Count * 0.99), sorted.Count - 1)];
    }

    // incident detection
    var incidents = new List<object>();
    bool wasDown = false;
    DateTime? downSince = null;

    foreach (var ping in pings)
    {
        if (!ping.IsUp && !wasDown)
        {
            wasDown = true;
            downSince = ping.Timestamp;
        }
        else if (ping.IsUp && wasDown)
        {
            wasDown = false;
            incidents.Add(new
            {
                from = downSince,
                to = ping.Timestamp,
                durationMinutes = Math.Round((ping.Timestamp - downSince!.Value).TotalMinutes, 1)
            });
            downSince = null;
        }
    }
    if (wasDown && downSince != null)
    {
        incidents.Add(new
        {
            from = downSince,
            to = (DateTime?)null,
            durationMinutes = Math.Round((DateTime.UtcNow - downSince.Value).TotalMinutes, 1)
        });
    }

    // anomaly detection
    var recentAvg = pings.Where(p => p.IsUp && p.Timestamp >= DateTime.UtcNow.AddHours(-2))
        .Select(p => (double)p.ResponseTimeMs).DefaultIfEmpty(0).Average();
    var overallAvg = sorted.DefaultIfEmpty(0).Average();
    var anomaly = overallAvg > 0 && recentAvg > overallAvg * 1.4;
    var anomalyPercent = overallAvg > 0 ? Math.Round((recentAvg - overallAvg) / overallAvg * 100, 0) : 0;

    // build Claude prompt
    var endpointName = await _db.MonitoredEndpoints
        .Where(e => e.Id == id)
        .Select(e => e.Name)
        .FirstOrDefaultAsync();

    var prompt = $"""
        You are an API monitoring assistant. Analyze this data for {endpointName} in the last 24 hours and write a 2-3 sentence plain English summary for a developer. Be concise and direct.

        - Total pings: {pings.Count}
        - Uptime: {Math.Round(pings.Count(p => p.IsUp) * 100.0 / pings.Count, 1)}%
        - p50 response time: {p50}ms
        - p95 response time: {p95}ms  
        - p99 response time: {p99}ms
        - Incidents in last 24h: {incidents.Count}
        - Anomaly detected: {anomaly} {(anomaly ? $"(recent avg {recentAvg:F0}ms vs overall avg {overallAvg:F0}ms, {anomalyPercent}% slower)" : "")}

        Write only the summary, no headers or bullet points.
        """;

    // call Claude API
    var summary = await CallClaudeAsync(prompt);

    return Ok(new { summary, incidents, p50, p95, p99, anomaly, anomalyPercent, recentAvg = Math.Round(recentAvg), overallAvg = Math.Round(overallAvg) });
}

private async Task<string> CallClaudeAsync(string prompt)
{
    try
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", _config["Claude__ApiKey"]);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 200,
            messages = new[] { new { role = "user", content = prompt } }
        };

        var response = await client.PostAsJsonAsync("https://api.anthropic.com/v1/messages", body);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("content")[0].GetProperty("text").GetString() ?? "Unable to generate summary.";
    }
    catch (Exception ex)
    {
        Console.WriteLine("Claude API error: " + ex.Message);
        return "Unable to generate summary at this time.";
    }
}
}