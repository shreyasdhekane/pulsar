namespace Pulsar.API.Models;

public class PingResult
{
    public int Id { get; set; }
    public int EndpointId { get; set; }
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public bool IsUp { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MonitoredEndpoint Endpoint { get; set; } = null!;
}