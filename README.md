# ⚡ Pulsar — Real-Time API Monitoring

A production-grade API health monitoring platform with real-time updates,
AI-powered insights, and a live world map of global ping activity.

**[Live Demo](https://pulsar-bay.vercel.app)** · Built with C# · Angular · PostgreSQL · SignalR

---

![Dashboard Screenshot](screenshot.png)

---

## Features

- **Real-time monitoring** — SignalR WebSocket updates, no page refresh needed
- **AI insights** — Claude API generates natural language health summaries per endpoint
- **Anomaly detection** — flags when response times deviate from baseline
- **Incident timeline** — automatic downtime detection with duration tracking
- **p50/p95/p99 analytics** — production-grade response time percentiles
- **Live world map** — animated global ping visualization with Leaflet.js
- **JWT authentication** — secure user accounts with private endpoint monitoring
- **Custom endpoints** — monitor any public URL instantly

## Stack

| Layer    | Tech                                            |
| -------- | ----------------------------------------------- |
| Backend  | C# · .NET 10 · ASP.NET Core · SignalR           |
| Database | PostgreSQL · Entity Framework Core              |
| Frontend | Angular 17 · TypeScript · Chart.js · Leaflet.js |
| AI       | Anthropic Claude API                            |
| Hosting  | Railway (backend) · Vercel (frontend)           |

## Running Locally

```bash
# Backend
cd Pulsar.API
dotnet run

# Frontend
cd pulsar-client
ng serve
```

## Architecture

Background `PingWorker` service pings all monitored endpoints every 60 seconds,
persists results to PostgreSQL, and broadcasts live updates to all connected
clients via SignalR hubs. Frontend subscribes to the hub and updates in real time
without polling.
