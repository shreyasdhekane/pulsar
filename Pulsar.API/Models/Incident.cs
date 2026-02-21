namespace Pulsar.API.Models;

public class Incident
{
    public int Id { get; set; }
    public int EndpointId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public MonitoredEndpoint Endpoint { get; set; } = null!;
}