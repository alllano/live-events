import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { CreateEventRequest } from '../common/models/create-event-request.model';
import { EventDetailResponse } from '../common/models/event-detail-response.model';
import { EventFilterRequest } from '../common/models/event-filter-request.model';
import { EventListItemResponse } from '../common/models/event-list-item-response.model';
import { HttpBaseService } from './http-base.service';

@Injectable({ providedIn: 'root' })
export class EventService {
  private readonly eventsSignal = signal<EventListItemResponse[]>([]);

  readonly events = this.eventsSignal.asReadonly();

  constructor(private readonly httpBaseService: HttpBaseService) {}

  /** Creates a new event. */
  createEvent(request: CreateEventRequest): Observable<EventDetailResponse> {
    return this.httpBaseService.post<EventDetailResponse>('events', request);
  }

  /** Retrieves a single event by id. */
  getEventById(id: number): Observable<EventDetailResponse> {
    return this.httpBaseService.get<EventDetailResponse>(`events/${id}`);
  }

  /** Retrieves events matching the given filters and updates the cached events signal. */
  getEvents(filter: EventFilterRequest): Observable<EventListItemResponse[]> {
    return this.httpBaseService
      .get<EventListItemResponse[]>('events', filter as Record<string, unknown>)
      .pipe(tap((events) => this.eventsSignal.set(events)));
  }
}
