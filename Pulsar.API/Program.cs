using Pulsar.API.Data;
using Pulsar.API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PulsarDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services container configuration
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHostedService<Pulsar.API.BackgroundServices.PingWorker>();

var app = builder.Build();
// Seed featured endpoints
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PulsarDbContext>();
    
    if (!db.MonitoredEndpoints.Any())
    {
        db.MonitoredEndpoints.AddRange(
            new MonitoredEndpoint { Name = "OpenAI", Url = "https://status.openai.com/api/v2/status.json", IsFeatured = true, IsPublic = true },
            new MonitoredEndpoint { Name = "GitHub", Url = "https://kctbh9vrtdwd.statuspage.io/api/v2/status.json", IsFeatured = true, IsPublic = true },
            new MonitoredEndpoint { Name = "Stripe", Url = "https://status.stripe.com/api/v2/summary.json", IsFeatured = true, IsPublic = true },
            new MonitoredEndpoint { Name = "Twilio", Url = "https://status.twilio.com/api/v2/status.json", IsFeatured = true, IsPublic = true },
            new MonitoredEndpoint { Name = "Cloudflare", Url = "https://www.cloudflarestatus.com/api/v2/status.json", IsFeatured = true, IsPublic = true }
        );
        await db.SaveChangesAsync();
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();