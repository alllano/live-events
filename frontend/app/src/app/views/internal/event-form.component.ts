import { Component, signal } from '@angular/core';
import { FormField, FormRoot, form, maxLength, minLength, required, schema, validate } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';

import { EventType } from '../../common/enums/event-type';
import { CreateEventRequest } from '../../common/models/create-event-request.model';
import { EventService } from '../../services/event.service';

interface EventFormModel {
  name: string;
  description: string;
  // Native <select> elements always work with strings; '0' represents "nothing selected".
  venueId: string;
  maxCapacity: number;
  startDate: string;
  endDate: string;
  price: number;
  eventTypeId: string;
}

interface SelectOption {
  id: number;
  name: string;
}

/** Mirrors CreateEventRequestValidator from the backend, as client-side UX validation (the backend remains the source of truth). */
const eventFormSchema = schema<EventFormModel>((path) => {
  required(path.name, { message: 'Name is required.' });
  minLength(path.name, 5, { message: 'Name must be between 5 and 100 characters long.' });
  maxLength(path.name, 100, { message: 'Name must be between 5 and 100 characters long.' });

  required(path.description, { message: 'Description is required.' });
  minLength(path.description, 10, { message: 'Description must be between 10 and 500 characters long.' });
  maxLength(path.description, 500, { message: 'Description must be between 10 and 500 characters long.' });

  validate(path.venueId, ({ value }) =>
    Number(value()) > 0 ? null : { kind: 'required', message: 'Venue is required.' }
  );

  required(path.maxCapacity, { message: 'MaxCapacity is required.' });
  validate(path.maxCapacity, ({ value }) =>
    value() > 0 ? null : { kind: 'positive', message: 'MaxCapacity must be a positive integer.' }
  );

  required(path.startDate, { message: 'StartDate is required.' });
  validate(path.startDate, ({ value }) =>
    new Date(value()) > new Date() ? null : { kind: 'futureDate', message: 'StartDate must be a future date.' }
  );

  required(path.endDate, { message: 'EndDate is required.' });
  validate(path.endDate, ({ value, valueOf }) => {
    const startDateValue = valueOf(path.startDate);
    return new Date(value()) > new Date(startDateValue)
      ? null
      : { kind: 'afterStartDate', message: 'EndDate must be later than StartDate.' };
  });

  required(path.price, { message: 'Price is required.' });
  validate(path.price, ({ value }) =>
    value() > 0 ? null : { kind: 'positive', message: 'Price must be a positive value.' }
  );

  validate(path.eventTypeId, ({ value }) =>
    Number(value()) > 0 ? null : { kind: 'required', message: 'EventTypeId is required.' }
  );
});

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [FormField, FormRoot],
  templateUrl: './event-form.component.html',
  styleUrl: './event-form.component.scss',
})
export class EventFormComponent {
  readonly venues: SelectOption[] = [
    { id: 1, name: 'Auditorio Central' },
    { id: 2, name: 'Sala Norte' },
    { id: 3, name: 'Arena Sur' },
  ];

  readonly eventTypes: SelectOption[] = Object.entries(EventType)
    .filter(([, value]) => typeof value === 'number')
    .map(([name, value]) => ({ id: value as EventType, name }));

  readonly submitting = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  private readonly model = signal<EventFormModel>({
    name: '',
    description: '',
    venueId: '0',
    maxCapacity: 0,
    startDate: '',
    endDate: '',
    price: 0,
    eventTypeId: '0',
  });

  readonly eventForm = form(this.model, eventFormSchema, {
    submission: {
      action: async (field) => {
        this.submitting.set(true);
        this.successMessage.set(null);
        this.errorMessage.set(null);

        try {
          const formValue = field().value();
          const request: CreateEventRequest = {
            ...formValue,
            venueId: Number(formValue.venueId),
            eventTypeId: Number(formValue.eventTypeId) as EventType,
          };
          const createdEvent = await firstValueFrom(this.eventService.createEvent(request));
          this.successMessage.set(`Event "${createdEvent.name}" was created successfully.`);
        } catch (error) {
          this.errorMessage.set((error as Error).message);
        } finally {
          this.submitting.set(false);
        }
      },
    },
  });

  constructor(private readonly eventService: EventService) {}
}
