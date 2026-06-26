import { Component, Input, signal } from '@angular/core';
import { FormField, FormRoot, email, form, min, required, schema } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';

import { CreateReservationRequest } from '../../common/models/create-reservation-request.model';
import { ReservationService } from '../../services/reservation.service';

interface ReservationFormModel {
  ticketQuantity: number;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
}

/** Mirrors CreateReservationRequestValidator from the backend, as client-side UX validation (the backend remains the source of truth). */
const reservationFormSchema = schema<ReservationFormModel>((path) => {
  required(path.ticketQuantity, { message: 'TicketQuantity is required.' });
  min(path.ticketQuantity, 1, { message: 'TicketQuantity must be at least 1.' });

  required(path.customerName, { message: 'CustomerName is required.' });

  required(path.customerEmail, { message: 'CustomerEmail is required.' });
  email(path.customerEmail, { message: 'CustomerEmail must be a valid email address.' });

  // customerPhone is optional, with no format validation, matching CreateReservationRequestValidator.
});

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  imports: [FormField, FormRoot],
  templateUrl: './reservation-form.component.html',
  styleUrl: './reservation-form.component.scss',
})
export class ReservationFormComponent {
  @Input({ required: true }) eventId!: number;

  readonly submitting = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  private readonly model = signal<ReservationFormModel>({
    ticketQuantity: 1,
    customerName: '',
    customerEmail: '',
    customerPhone: '',
  });

  readonly reservationForm = form(this.model, reservationFormSchema, {
    submission: {
      action: async (field) => {
        this.submitting.set(true);
        this.successMessage.set(null);
        this.errorMessage.set(null);

        try {
          const formValue = field().value();
          const request: CreateReservationRequest = {
            eventId: this.eventId,
            ticketQuantity: formValue.ticketQuantity,
            customerName: formValue.customerName,
            customerEmail: formValue.customerEmail,
            customerPhone: formValue.customerPhone || undefined,
          };

          // ReservationCode is only generated on payment confirmation (FR-04); the reservation is created in PendingPayment status here.
          const createdReservation = await firstValueFrom(this.reservationService.createReservation(request));
          this.successMessage.set(
            `Reservation created for ${createdReservation.ticketQuantity} ticket(s), total ${createdReservation.totalPrice}. It is pending payment confirmation.`
          );
        } catch (error) {
          this.errorMessage.set((error as Error).message);
        } finally {
          this.submitting.set(false);
        }
      },
    },
  });

  constructor(private readonly reservationService: ReservationService) {}
}
