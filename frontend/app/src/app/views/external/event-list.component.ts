import { CommonModule } from '@angular/common';
import { Component, OnInit, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { EventType } from '../../common/enums/event-type';
import { EventFilterRequest } from '../../common/models/event-filter-request.model';
import { EventService } from '../../services/event.service';

interface EventTypeOption {
  id: EventType;
  name: string;
}

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
})
export class EventListComponent implements OnInit {
  private readonly eventService = inject(EventService);
  readonly events = this.eventService.events;
  readonly loading = signal(false);

  /** Built from the EventType enum so the select options stay in sync with it automatically. */
  readonly eventTypes: EventTypeOption[] = Object.entries(EventType)
    .filter(([, value]) => typeof value === 'number')
    .map(([name, value]) => ({ id: value as EventType, name }));

  titleSearch = '';
  eventTypeId: EventType | null = null;

  constructor() {}

  ngOnInit(): void {
    this.loadEvents();
  }

  /** Reloads the event list using the current filter values, discarding any unset (empty/null) filter. */
  loadEvents(): void {
    const filter: EventFilterRequest = {
      titleSearch: this.titleSearch || undefined,
      eventTypeId: this.eventTypeId ?? undefined,
    };

    this.loading.set(true);
    this.eventService.getEvents(filter).subscribe({
      next: () => this.loading.set(false),
      error: () => this.loading.set(false),
    });
  }
}
