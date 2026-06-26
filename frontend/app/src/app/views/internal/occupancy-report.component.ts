import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { OccupancyReportResponse } from '../../common/models/occupancy-report-response.model';
import { EventService } from '../../services/event.service';

@Component({
  selector: 'app-occupancy-report',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './occupancy-report.component.html',
  styleUrl: './occupancy-report.component.scss',
})
export class OccupancyReportComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly eventService = inject(EventService);

  readonly report = signal<OccupancyReportResponse | null>(null);
  readonly loading = signal(false);
  readonly notFound = signal(false);

  ngOnInit(): void {
    this.route.paramMap.subscribe((paramMap) => {
      const eventId = Number(paramMap.get('id'));
      this.loadReport(eventId);
    });
  }

  private loadReport(eventId: number): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.report.set(null);

    this.eventService.getOccupancyReport(eventId).subscribe({
      next: (report) => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }
}
