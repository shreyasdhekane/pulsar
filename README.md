# Pulsar v1.0 — First Live Release

**Released:** February 22, 2026  
**Live Demo:** https://pulsar-of1bjwzg3-shreyasdhekanes-projects.vercel.app  
**Backend:** https://pulsar-production-a199.up.railway.app

---

## What is Pulsar?

Pulsar is a real-time API health monitoring dashboard. It continuously pings external APIs and services, tracks response times, and displays live status updates — all in a clean, professional dark UI.

---

## What's in v0.8

### Core Features

- **Real-time monitoring** — SignalR-powered live updates, no page refresh needed
- **5 pre-monitored APIs** — OpenAI, GitHub, Stripe, Twilio, Cloudflare
- **Custom endpoint monitoring** — add any public URL and start tracking it instantly
- **Hero stats bar** — total endpoints, live count, average response time, incidents at a glance
- **Sparkline graphs** — per-card response time trend at a glance

### Detail Page

- 24-hour uptime timeline bar (GitHub-style)
- Response time graph with per-ping color coding (green = up, red = down)
- Stats cards: uptime %, average, fastest, and slowest response times

### Design

- Professional dark UI inspired by Vercel and modern SaaS dashboards
- Glassmorphism cards with gradient accents
- DM Mono + Sora typography
- Animated gradient mesh background

### Infrastructure

- Backend hosted on **Railway** (.NET 10 + PostgreSQL)
- Frontend hosted on **Vercel** (Angular 17)
- Auto-migrations on startup
- Background PingWorker running every 60 seconds

---

## Known Limitations

- No authentication yet — all endpoints are public
- No email/Slack alerts on downtime
- Mobile layout not fully optimized

---

## What's Next (v2.0)

- User authentication
- Email/Slack downtime alerts
- World map with live ping visualization
- Mobile responsive polish
- Incident history timeline
