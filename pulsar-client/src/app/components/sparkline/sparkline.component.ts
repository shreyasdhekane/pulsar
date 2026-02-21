import { Component, Input, OnInit, OnChanges, ElementRef, ViewChild } from '@angular/core';
import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Filler,
} from 'chart.js';

Chart.register(LineController, LineElement, PointElement, LinearScale, CategoryScale, Filler);

@Component({
  selector: 'app-sparkline',
  standalone: true,
  template: `<canvas #chart></canvas>`,
  styles: [
    `
      canvas {
        width: 100% !important;
        height: 60px !important;
      }
    `,
  ],
})
export class SparklineComponent implements OnInit, OnChanges {
  @Input() pings: any[] = [];
  @Input() isUp: boolean = true;
  @ViewChild('chart', { static: true }) chartRef!: ElementRef;
  private chart!: Chart;

  ngOnInit() {
    this.buildChart();
  }
  ngOnChanges() {
    if (this.chart) this.updateChart();
  }

  buildChart() {
    const color = this.isUp ? '#00ff88' : '#ff4444';
    const ctx = this.chartRef.nativeElement.getContext('2d');
    const labels = this.pings.map((_, i) => i.toString());
    const data = this.pings.map((p) => p.responseTimeMs);

    this.chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            data,
            borderColor: color,
            borderWidth: 2,
            pointRadius: 0,
            fill: true,
            backgroundColor: this.isUp ? 'rgba(0,255,136,0.08)' : 'rgba(255,68,68,0.08)',
            tension: 0.4,
          },
        ],
      },
      options: {
        responsive: true,
        animation: { duration: 400 },
        plugins: { legend: { display: false } },
        scales: {
          x: { display: false },
          y: { display: false },
        },
      },
    });
  }

  updateChart() {
    const color = this.isUp ? '#00ff88' : '#ff4444';
    this.chart.data.labels = this.pings.map((_, i) => i.toString());
    this.chart.data.datasets[0].data = this.pings.map((p) => p.responseTimeMs);
    this.chart.data.datasets[0].borderColor = color;
    this.chart.update();
  }
}
