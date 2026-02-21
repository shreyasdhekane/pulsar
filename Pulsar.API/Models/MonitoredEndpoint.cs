namespace Pulsar.API.Models;

public class MonitoredEndpoint
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; } = 60;
    public bool IsPublic { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
    public ICollection<PingResult> PingResults { get; set; } = new List<PingResult>();
}