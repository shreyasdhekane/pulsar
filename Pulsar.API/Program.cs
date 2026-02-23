using Pulsar.API.Data;
using Pulsar.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PulsarDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHostedService<Pulsar.API.BackgroundServices.PingWorker>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
       policy.WithOrigins(
            "http://localhost:4200",
            "https://pulsar-bay.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); 
    });
});
builder.Services.AddSignalR();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt__Issuer"],
            ValidAudience = builder.Configuration["Jwt__Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt__Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PulsarDbContext>();
    try
        {
            db.Database.Migrate();
        }
    catch (Exception ex)
        {
            Console.WriteLine("Migration failed: " + ex.Message);
        }
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
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<Pulsar.API.Hubs.PulsarHub>("/hubs/pulsar");
app.MapControllers();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");