import { Routes } from '@angular/router';

import { EventDetailComponent } from './views/external/event-detail.component';
import { EventListComponent } from './views/external/event-list.component';
import { EventFormComponent } from './views/internal/event-form.component';
import { OccupancyReportComponent } from './views/internal/occupancy-report.component';
import { ReservationListComponent } from './views/internal/reservation-list.component';
import { ReservationCancelComponent } from './views/shared/reservation-cancel.component';

export const routes: Routes = [
  // External (public portal)
  { path: '', redirectTo: 'events', pathMatch: 'full' },
  { path: 'events', component: EventListComponent },
  { path: 'events/:id', component: EventDetailComponent },
  { path: 'my-reservations', component: ReservationCancelComponent },

  // Internal (admin panel)
  { path: 'admin/events/create', component: EventFormComponent },
  { path: 'admin/events/:id/reservations', component: ReservationListComponent },
  { path: 'admin/events/:id/report', component: OccupancyReportComponent },
];
