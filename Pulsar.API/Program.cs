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
        var jwtKey = builder.Configuration["Jwt__Key"];
        var jwtIssuer = builder.Configuration["Jwt__Issuer"];
        var jwtAudience = builder.Configuration["Jwt__Audience"];
        
        // Log configuration status
        Console.WriteLine($"JWT Key configured: {!string.IsNullOrEmpty(jwtKey)}");
        Console.WriteLine($"JWT Issuer: {jwtIssuer ?? "NOT SET"}");
        Console.WriteLine($"JWT Audience: {jwtAudience ?? "NOT SET"}");
        
        // Fallback for development if not configured
        if (string.IsNullOrEmpty(jwtKey))
        {
            Console.WriteLine("WARNING: Using development JWT key");
            jwtKey = "16e7c32cd24278ebffd6908a7d853367bf5181739969bade865e0fb4b549a472";
        }
        
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer ?? "pulsar-api",
            ValidAudience = jwtAudience ?? "pulsar-client",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
        

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/pulsar"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Database migration and seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PulsarDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("Migration successful");
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
            new MonitoredEndpoint { Name = "Vercel", Url = "https://www.vercel-status.com/api/v2/status.json", IsFeatured = true, IsPublic = true },
            new MonitoredEndpoint { Name = "Twilio", Url = "https://status.twilio.com/api/v2/status.json", IsFeatured = true, IsPublic = true },
            new MonitoredEndpoint { Name = "Cloudflare", Url = "https://www.cloudflarestatus.com/api/v2/status.json", IsFeatured = true, IsPublic = true }
        );
        await db.SaveChangesAsync();
        Console.WriteLine("Seeding successful");
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