import { Routes } from '@angular/router';

import { EventDetailComponent } from './views/external/event-detail.component';
import { EventListComponent } from './views/external/event-list.component';
import { EventFormComponent } from './views/internal/event-form.component';
import { ReservationListComponent } from './views/internal/reservation-list.component';

export const routes: Routes = [
  // External (public portal)
  { path: '', redirectTo: 'events', pathMatch: 'full' },
  { path: 'events', component: EventListComponent },
  { path: 'events/:id', component: EventDetailComponent },

  // Internal (admin panel)
  { path: 'admin/events/create', component: EventFormComponent },
  { path: 'admin/events/:id/reservations', component: ReservationListComponent },
];
