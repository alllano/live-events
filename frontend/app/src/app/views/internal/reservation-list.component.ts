import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ReservationResponse } from '../../common/models/reservation-response.model';
import { ReservationService } from '../../services/reservation.service';

@Component({
  selector: 'app-reservation-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reservation-list.component.html',
  styleUrl: './reservation-list.component.scss',
})
export class ReservationListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly reservationService = inject(ReservationService);

  private eventId = 0;

  readonly reservations = signal<ReservationResponse[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.route.paramMap.subscribe((paramMap) => {
      this.eventId = Number(paramMap.get('id'));
      this.loadReservations();
    });
  }

  /** Confirms payment for a PendingPayment reservation and reloads the list to reflect its new status. */
  confirmPayment(reservationId: number): void {
    this.runAction(this.reservationService.confirmPayment(reservationId));
  }

  /** Cancels a Confirmed reservation and reloads the list to reflect its new status. */
  cancelReservation(reservationId: number): void {
    this.runAction(this.reservationService.cancelReservation(reservationId));
  }

  /** Releases a Lost reservation back to Cancelled and reloads the list to reflect its new status. */
  releaseReservation(reservationId: number): void {
    this.runAction(this.reservationService.releaseReservation(reservationId));
  }

  private runAction(action: ReturnType<ReservationService['confirmPayment']>): void {
    this.errorMessage.set(null);

    action.subscribe({
      next: () => this.loadReservations(),
      error: (error: Error) => this.errorMessage.set(error.message),
    });
  }

  private loadReservations(): void {
    this.loading.set(true);

    this.reservationService.getReservationsByEventId(this.eventId).subscribe({
      next: (reservations) => {
        this.reservations.set(reservations);
        this.loading.set(false);
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.loading.set(false);
      },
    });
  }
}
