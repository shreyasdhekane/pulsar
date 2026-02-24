# Pulsar v2.0 — Intelligent API Health Monitoring

**Released:** March 2026  
**Live Demo:** https://pulsar-bay.vercel.app  
**Backend:** https://pulsar-production-a199.up.railway.app

---

## 🚀 What is Pulsar?

Pulsar is a real-time API health monitoring platform that continuously pings external services, tracks response times, detects downtime, and visualizes system health — all in a clean, production-grade dashboard.

Built to simulate how modern DevOps monitoring systems work — powered by a live backend, persistent storage, and real-time updates.

---

## ✨ What’s New in v2.0

Version 2.0 transforms Pulsar from a monitoring dashboard into a structured monitoring platform.

- 🔐 User authentication implemented
- 🌍 World map with live ping visualization
- 📜 Incident history timeline
- 📱 Fully responsive mobile layout
- ⚡ Performance improvements and UI polish
- 🧠 Backend structure improvements for scalability

---

## 🧩 Core Features

### ⚡ Real-Time Monitoring

- SignalR-powered live updates .
- Background PingWorker running every 60 seconds
- Instant status transitions (UP / DOWN)

### 🌐 Pre-Monitored APIs

Currently tracking:

- OpenAI
- GitHub
- Stripe
- Twilio
- Cloudflare

### ➕ Custom Endpoint Monitoring

- Add any public URL
- Immediate tracking begins
- Persistent storage in PostgreSQL

### 📊 Hero Stats Bar

- Total endpoints
- Live endpoints
- Average response time
- Active incidents

### 📈 Visual Analytics

- Sparkline response time graphs (per endpoint)
- 24-hour uptime timeline (GitHub-style)
- Color-coded ping history (green = up, red = down)
- Response time graph with detailed metrics

### 📜 Incident History Timeline

- Structured downtime event tracking
- Start time, end time, duration
- Historical reliability visibility

### 🌍 Live World Map

- Real-time ping visualization
- Geographic representation of endpoint health

---

## 🎨 Design System

- Professional dark UI inspired by modern SaaS dashboards
- Glassmorphism cards with gradient accents
- DM Mono + Sora typography
- Animated gradient mesh background
- Fully responsive layout (mobile + desktop)

---

## 🏗 Architecture

### Backend

- .NET 10
- PostgreSQL
- Entity Framework Core
- SignalR for real-time communication
- Hosted on Railway
- Auto-migrations on startup
- Background worker service for continuous pinging

### Frontend

- Angular 17
- TypeScript
- RxJS
- Hosted on Vercel

---

## 🛠 Infrastructure Overview

- Live deployment (frontend + backend)
- Persistent cloud database
- Real-time WebSocket communication
- Production-ready structure

---

## 📌 Roadmap (v3.0 Ideas)

- Multi-region ping testing
- Public status page generation
- Advanced analytics dashboard
- Rate-limiting & performance benchmarking
- Monitoring groups / teams support

---

## 🧠 Why Pulsar?

Pulsar is a practical full-stack systems project that demonstrates:

- Real-time systems design
- Background job processing
- Cloud deployment
- Database modeling
- API health analytics
- Modern frontend architecture
- Scalable backend structure

---

## 🛰 Status

Pulsar v2.0 is fully live and production deployed.

More iterations coming.
