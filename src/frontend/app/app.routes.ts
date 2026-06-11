import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('@features/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'events',
    loadComponent: () =>
      import('@features/events/events-list/events-list.component').then(
        (m) => m.EventsListComponent,
      ),
  },
  {
    path: 'events/create',
    loadComponent: () =>
      import('@features/events/create-event/create-event.component').then(
        (m) => m.CreateEventComponent,
      ),
  },
  { path: '**', redirectTo: '' },
];
