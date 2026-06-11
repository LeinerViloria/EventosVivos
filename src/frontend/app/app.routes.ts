import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'events/create', pathMatch: 'full' },
  {
    path: 'events/create',
    loadComponent: () =>
      import('@features/events/create-event/create-event.component').then(
        (m) => m.CreateEventComponent,
      ),
  },
];
