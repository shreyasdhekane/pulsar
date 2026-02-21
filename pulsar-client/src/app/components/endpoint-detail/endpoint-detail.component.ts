import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Filler,
  Tooltip,
} from 'chart.js';

Chart.register(
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Filler,
  Tooltip,
);

@Component({
  selector: 'app-endpoint-detail',
  standalone: true,
  imports: [],
  templateUrl: './endpoint-detail.component.html',
  styleUrl: './endpoint-detail.component.scss',
})
export class EndpointDetailComponent implements OnInit {
  endpoint: any = null;
  loading = true;
  uptimeBars: { status: string; label: string }[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    this.apiService.getEndpointDetail(Number(id)).subscribe({
      next: (data) => {
        this.endpoint = data;
        this.loading = false;
        this.buildUptimeBars();
        this.cdr.detectChanges();
        setTimeout(() => this.buildChart(), 100);
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      },
    });
  }

  buildUptimeBars() {
    const bars = [];
    const now = new Date();
    for (let i = 47; i >= 0; i--) {
      const slotStart = new Date(now.getTime() - (i + 1) * 30 * 60000);
      const slotEnd = new Date(now.getTime() - i * 30 * 60000);
      const pingsInSlot = this.endpoint.last24Hours.filter((p: any) => {
        const t = new Date(p.timestamp);
        return t >= slotStart && t < slotEnd;
      });
      let status = 'unknown';
      if (pingsInSlot.length > 0) {
        status = pingsInSlot.every((p: any) => p.isUp) ? 'up' : 'down';
      }
      bars.push({
        status,
        label: slotStart.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      });
    }
    this.uptimeBars = bars;
  }

  buildChart() {
    const canvas = document.getElementById('detailChart') as HTMLCanvasElement;
    if (!canvas || !this.endpoint?.last24Hours?.length) return;

    const labels = this.endpoint.last24Hours.map((p: any) =>
      new Date(p.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    );
    const data = this.endpoint.last24Hours.map((p: any) => p.responseTimeMs);
    const pointColors = this.endpoint.last24Hours.map((p: any) => (p.isUp ? '#00ff88' : '#ff4444'));

    new Chart(canvas, {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            label: 'Response Time (ms)',
            data,
            borderColor: '#6c63ff',
            borderWidth: 2,
            pointRadius: 4,
            pointBackgroundColor: pointColors,
            pointBorderColor: pointColors,
            fill: true,
            backgroundColor: 'rgba(108,99,255,0.06)',
            tension: 0.4,
          },
        ],
      },
      options: {
        responsive: true,
        plugins: {
          legend: { display: false },
          tooltip: {
            mode: 'index',
            intersect: false,
            backgroundColor: '#0e0e14',
            borderColor: 'rgba(255,255,255,0.1)',
            borderWidth: 1,
            titleColor: '#888',
            bodyColor: '#fff',
            padding: 12,
          },
        },
        scales: {
          x: {
            ticks: { color: '#333', maxTicksLimit: 8, font: { family: 'DM Mono' } },
            grid: { color: 'rgba(255,255,255,0.03)' },
            border: { color: 'rgba(255,255,255,0.05)' },
          },
          y: {
            ticks: { color: '#333', font: { family: 'DM Mono' } },
            grid: { color: 'rgba(255,255,255,0.03)' },
            border: { color: 'rgba(255,255,255,0.05)' },
          },
        },
      },
    });
  }

  getStatusColor(): string {
    if (!this.endpoint?.latestPing) return '#888';
    return this.endpoint.latestPing.isUp ? '#00ff88' : '#ff4444';
  }

  goBack() {
    this.router.navigate(['/']);
  }
}
