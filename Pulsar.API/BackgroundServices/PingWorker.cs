using Microsoft.EntityFrameworkCore;
using Pulsar.API.Data;
using Pulsar.API.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using Pulsar.API.Hubs;

namespace Pulsar.API.BackgroundServices;

public class PingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PingWorker> _logger;
    private readonly HttpClient _httpClient;
    private readonly IHubContext<PulsarHub> _hubContext;

    public PingWorker(IServiceScopeFactory scopeFactory, ILogger<PingWorker> logger, IHubContext<PulsarHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _hubContext = hubContext;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PingWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await PingAllEndpointsAsync();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task PingAllEndpointsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PulsarDbContext>();

        var endpoints = await db.MonitoredEndpoints.ToListAsync();

        foreach (var endpoint in endpoints)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(endpoint.Url);
                stopwatch.Stop();

                var result = new PingResult
                {
                    EndpointId = endpoint.Id,
                    StatusCode = (int)response.StatusCode,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    IsUp = response.IsSuccessStatusCode,
                    Timestamp = DateTime.UtcNow
                };

                db.PingResults.Add(result);
                _logger.LogInformation(
                    "Pinged {Name}: {StatusCode} in {Ms}ms",
                    endpoint.Name, result.StatusCode, result.ResponseTimeMs);

                await _hubContext.Clients.All.SendAsync("ReceivePingResult", new {
                    endpointId = endpoint.Id,
                    statusCode = result.StatusCode,
                    responseTimeMs = result.ResponseTimeMs,
                    isUp = result.IsUp,
                    timestamp = result.Timestamp
                });
            }
            catch (Exception ex)
            {
                var result = new PingResult
                {
                    EndpointId = endpoint.Id,
                    StatusCode = 0,
                    ResponseTimeMs = 0,
                    IsUp = false,
                    Timestamp = DateTime.UtcNow
                };
                db.PingResults.Add(result);
                _logger.LogWarning("Failed to ping {Name}: {Error}", endpoint.Name, ex.Message);
            }
        }

        await db.SaveChangesAsync();
    }
}