using Microsoft.EntityFrameworkCore;
using Pulsar.API.Models;

namespace Pulsar.API.Data;

public class PulsarDbContext : DbContext
{
    public PulsarDbContext(DbContextOptions<PulsarDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<MonitoredEndpoint> MonitoredEndpoints => Set<MonitoredEndpoint>();
    public DbSet<PingResult> PingResults => Set<PingResult>();
    public DbSet<Incident> Incidents => Set<Incident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MonitoredEndpoint>()
            .HasOne(e => e.User)
            .WithMany(u => u.Endpoints)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PingResult>()
            .HasOne(p => p.Endpoint)
            .WithMany(e => e.PingResults)
            .HasForeignKey(p => p.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}