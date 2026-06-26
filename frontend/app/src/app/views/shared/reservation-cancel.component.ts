import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ReservationResponse } from '../../common/models/reservation-response.model';
import { ReservationService } from '../../services/reservation.service';

@Component({
  selector: 'app-reservation-cancel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation-cancel.component.html',
  styleUrl: './reservation-cancel.component.scss',
})
export class ReservationCancelComponent {
  private readonly reservationService = inject(ReservationService);

  customerEmail = '';

  readonly reservations = signal<ReservationResponse[]>([]);
  readonly loading = signal(false);
  readonly searched = signal(false);
  readonly errorMessage = signal<string | null>(null);

  /** Searches all reservations for the entered email. */
  findReservations(): void {
    if (!this.customerEmail) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.reservationService.getReservationsByEmail(this.customerEmail).subscribe({
      next: (reservations) => {
        this.reservations.set(reservations);
        this.searched.set(true);
        this.loading.set(false);
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.loading.set(false);
      },
    });
  }

  /** Cancels a Confirmed reservation and reloads the search to reflect its new status. */
  cancelReservation(reservationId: number): void {
    this.errorMessage.set(null);

    this.reservationService.cancelReservation(reservationId).subscribe({
      next: () => this.findReservations(),
      error: (error: Error) => this.errorMessage.set(error.message),
    });
  }
}
