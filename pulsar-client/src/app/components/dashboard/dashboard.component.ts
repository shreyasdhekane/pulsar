import { Component, OnInit, OnDestroy, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { DecimalPipe } from '@angular/common';
import { SignalrService } from '../../services/signalr.service';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SparklineComponent } from '../sparkline/sparkline.component';
import { AuthService } from '../../services/auth.service';
import { AuthModalComponent } from '../auth-modal/auth-modal.component';
import * as L from 'leaflet';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [SparklineComponent, DecimalPipe, FormsModule, AuthModalComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  endpoints: any[] = [];
  loading = true;
  stats: any = null;
  showAuthModal = false;
  showModal = false;
  newName = '';
  newUrl = '';
  tickerItems: any[] = [];

  private map: L.Map | null = null;
  private pingLocations = [
    { lat: 40.7128, lng: -74.006, label: 'New York' },
    { lat: 51.5074, lng: -0.1278, label: 'London' },
    { lat: 35.6762, lng: 139.6503, label: 'Tokyo' },
    { lat: 48.8566, lng: 2.3522, label: 'Paris' },
    { lat: 1.3521, lng: 103.8198, label: 'Singapore' },
    { lat: -33.8688, lng: 151.2093, label: 'Sydney' },
    { lat: 37.7749, lng: -122.4194, label: 'San Francisco' },
    { lat: 52.52, lng: 13.405, label: 'Berlin' },
  ];

  constructor(
    public auth: AuthService,
    private apiService: ApiService,
    private signalrService: SignalrService,
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit() {
    this.apiService.getStats().subscribe((data) => {
      this.stats = data;
      this.cdr.detectChanges();
    });

    this.apiService.getFeaturedEndpoints().subscribe({
      next: (data) => {
        this.endpoints = data;
        this.loading = false;
        this.tickerItems = data.map((e: any) => ({
          name: e.name,
          isUp: e.latestPing?.isUp ?? true,
          responseTimeMs: e.latestPing?.responseTimeMs ?? 0,
        }));
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to fetch endpoints', err);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });

    this.signalrService.startConnection();
    this.signalrService.pingReceived$.subscribe((ping) => {
      const endpoint = this.endpoints.find((e) => e.id === ping.endpointId);
      if (endpoint) {
        endpoint.latestPing = ping;
        endpoint.uptimePercent = ping.isUp ? 100 : 0;
        // update ticker
        const tickerItem = this.tickerItems.find((t) => t.name === endpoint.name);
        if (tickerItem) {
          tickerItem.isUp = ping.isUp;
          tickerItem.responseTimeMs = ping.responseTimeMs;
        }
        // flash a random map dot
        this.flashRandomPing(ping.isUp);
        this.cdr.detectChanges();
      }
    });
  }

  ngAfterViewInit() {
    setTimeout(() => this.initMap(), 500);
  }

  ngOnDestroy() {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }

  initMap() {
    const mapEl = document.getElementById('world-map');
    if (!mapEl || this.map) return;

    this.map = L.map('world-map', {
      center: [20, 0],
      zoom: 2,
      zoomControl: false,
      attributionControl: false,
      dragging: false,
      scrollWheelZoom: false,
      doubleClickZoom: false,
    });

    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
      maxZoom: 19,
    }).addTo(this.map);

    this.pingLocations.forEach((loc) => {
      L.circleMarker([loc.lat, loc.lng], {
        radius: 3,
        fillColor: '#00ff88',
        color: '#00ff88',
        weight: 1,
        fillOpacity: 0.5,
      }).addTo(this.map!);
    });

    this.startRandomPings();
  }

  startRandomPings() {
    setInterval(() => {
      this.flashRandomPing(Math.random() > 0.1);
    }, 1500);
  }

  flashRandomPing(isUp: boolean) {
    if (!this.map) return;
    const loc = this.pingLocations[Math.floor(Math.random() * this.pingLocations.length)];
    const color = '#00ff88';

    const circle = L.circleMarker([loc.lat, loc.lng], {
      radius: 3,
      fillColor: color,
      color: color,
      weight: 1,
      fillOpacity: 0.9,
    }).addTo(this.map);

    let radius = 3;
    let opacity = 0.9;
    const animate = setInterval(() => {
      radius += 1.5;
      opacity -= 0.12;
      circle.setRadius(radius);
      circle.setStyle({ fillOpacity: opacity, opacity });
      if (opacity <= 0) {
        clearInterval(animate);
        this.map?.removeLayer(circle);
      }
    }, 50);
  }

  scrollToCards() {
    document.getElementById('cards-section')?.scrollIntoView({ behavior: 'smooth' });
  }

  getTimeAgo(timestamp: string): string {
    if (!timestamp) return 'Never';
    const seconds = Math.floor((Date.now() - new Date(timestamp).getTime()) / 1000);
    if (seconds < 60) return `${seconds}s ago`;
    return `${Math.floor(seconds / 60)}m ago`;
  }

  getStatusColor(endpoint: any): string {
    if (!endpoint.latestPing) return '#888888';
    return endpoint.latestPing.isUp ? '#00ff88' : '#ff4444';
  }

  getStatusLabel(endpoint: any): string {
    if (!endpoint.latestPing) return 'Unknown';
    return endpoint.latestPing.isUp ? 'Operational' : 'Down';
  }

  submitEndpoint() {
    if (!this.newName || !this.newUrl) return;
    this.apiService.addCustomEndpoint(this.newName, this.newUrl).subscribe({
      next: (endpoint) => {
        endpoint.recentPings = [];
        endpoint.latestPing = null;
        endpoint.uptimePercent = 0;
        this.endpoints.push(endpoint);
        this.closeModal();
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to add endpoint', err),
    });
  }

  goToDetail(id: number) {
    this.router.navigate(['/endpoint', id]);
  }

  openModal() {
    this.showModal = true;
  }
  closeModal() {
    this.showModal = false;
    this.newName = '';
    this.newUrl = '';
  }

  onAddEndpointClick() {
    if (!this.auth.isLoggedIn()) {
      this.showAuthModal = true;
    } else {
      this.showModal = true;
    }
  }

  onAuthSuccess() {
    this.showAuthModal = false;
    this.showModal = true;
  }

  getBarHeight(responseTimeMs: number, pings: any[]): number {
    const max = Math.max(...pings.map((p: any) => p.responseTimeMs), 1);
    return Math.max((responseTimeMs / max) * 40, 4);
  }
}
