import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { DecimalPipe } from '@angular/common';
import { SignalrService } from '../../services/signalr.service';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SparklineComponent } from '../sparkline/sparkline.component';
import { AuthService } from '../../services/auth.service';
import { AuthModalComponent } from '../auth-modal/auth-modal.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [SparklineComponent, DecimalPipe, FormsModule, AuthModalComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  endpoints: any[] = [];
  loading = true;
  stats: any = null;
  showAuthModal = false;
  showModal = false;
  newName = '';
  newUrl = '';

  openModal() {
    this.showModal = true;
  }
  closeModal() {
    this.showModal = false;
    this.newName = '';
    this.newUrl = '';
  }

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
        this.cdr.detectChanges();
      }
    });
  }
  getBarHeight(responseTimeMs: number, pings: any[]): number {
    const max = Math.max(...pings.map((p: any) => p.responseTimeMs), 1);
    return Math.max((responseTimeMs / max) * 40, 4);
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
}
