import { Routes } from '@angular/router';

import { EventFormComponent } from './views/internal/event-form.component';
import { EventListComponent } from './views/external/event-list.component';

export const routes: Routes = [
  // External (public portal)
  { path: '', redirectTo: 'events', pathMatch: 'full' },
  { path: 'events', component: EventListComponent },

  // Internal (admin panel)
  { path: 'admin/events/create', component: EventFormComponent },
];
