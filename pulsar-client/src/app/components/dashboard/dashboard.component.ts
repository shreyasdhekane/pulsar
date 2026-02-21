import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  endpoints: any[] = [];
  loading = true;

  constructor(
    private apiService: ApiService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
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
  }

  getStatusColor(endpoint: any): string {
    if (!endpoint.latestPing) return '#888888';
    return endpoint.latestPing.isUp ? '#00ff88' : '#ff4444';
  }

  getStatusLabel(endpoint: any): string {
    if (!endpoint.latestPing) return 'Unknown';
    return endpoint.latestPing.isUp ? 'Operational' : 'Down';
  }
}
