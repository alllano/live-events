import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { EventDetailResponse } from '../../common/models/event-detail-response.model';
import { EventService } from '../../services/event.service';
import { ReservationFormComponent } from './reservation-form.component';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule, ReservationFormComponent, RouterLink],
  templateUrl: './event-detail.component.html',
  styleUrl: './event-detail.component.scss',
})
export class EventDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly eventService = inject(EventService);

  readonly event = signal<EventDetailResponse | null>(null);
  readonly loading = signal(false);
  readonly notFound = signal(false);

  ngOnInit(): void {
    this.route.paramMap.subscribe((paramMap) => {
      const id = Number(paramMap.get('id'));
      this.loadEvent(id);
    });
  }

  /** Returns whether the event still accepts reservations: it must be in its effective Active status and have available tickets. */
  canReserve(): boolean {
    const currentEvent = this.event();
    return !!currentEvent && currentEvent.eventStatusName === 'Active' && currentEvent.availableTickets > 0;
  }

  private loadEvent(id: number): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.event.set(null);

    this.eventService.getEventById(id).subscribe({
      next: (eventDetail) => {
        this.event.set(eventDetail);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }
}
