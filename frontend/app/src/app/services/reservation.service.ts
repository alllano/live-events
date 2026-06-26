import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { CreateReservationRequest } from '../common/models/create-reservation-request.model';
import { ReservationResponse } from '../common/models/reservation-response.model';
import { HttpBaseService } from './http-base.service';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  constructor(private readonly httpBaseService: HttpBaseService) {}

  /** Creates a reservation in PendingPayment status for the given event and customer. */
  createReservation(request: CreateReservationRequest): Observable<ReservationResponse> {
    return this.httpBaseService.post<ReservationResponse>('reservations', request);
  }

  /** Retrieves all reservations for the given event. */
  getReservationsByEventId(eventId: number): Observable<ReservationResponse[]> {
    return this.httpBaseService.get<ReservationResponse[]>('reservations', { eventId });
  }

  /** Confirms payment for a PendingPayment reservation. */
  confirmPayment(id: number): Observable<ReservationResponse> {
    return this.httpBaseService.patch<ReservationResponse>(`reservations/${id}/confirm`);
  }

  /** Cancels a Confirmed reservation. */
  cancelReservation(id: number): Observable<ReservationResponse> {
    return this.httpBaseService.patch<ReservationResponse>(`reservations/${id}/cancel`);
  }

  /** Releases a Lost reservation's tickets by transitioning it to Cancelled. */
  releaseReservation(id: number): Observable<ReservationResponse> {
    return this.httpBaseService.patch<ReservationResponse>(`reservations/${id}/release`);
  }
}
