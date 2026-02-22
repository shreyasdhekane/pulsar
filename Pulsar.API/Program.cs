using Pulsar.API.Data;
using Pulsar.API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PulsarDbContext>(options =>
{
    var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:ConnectionString") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHostedService<Pulsar.API.BackgroundServices.PingWorker>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddSignalR();



var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PulsarDbContext>();
    db.Database.Migrate();
}

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
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngular");
app.MapHub<Pulsar.API.Hubs.PulsarHub>("/hubs/pulsar");
app.MapControllers();
app.Run();