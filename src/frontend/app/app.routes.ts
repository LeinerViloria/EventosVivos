import { Routes } from '@angular/router';
import { authGuard } from '@core/guards/auth.guard';
import { permissionGuard } from '@core/guards/permission.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('@features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('@features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('@features/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'events',
    canActivate: [authGuard],
    loadComponent: () =>
      import('@features/events/events-list/events-list.component').then(
        (m) => m.EventsListComponent,
      ),
  },
  {
    path: 'events/create',
    canActivate: [authGuard, permissionGuard('events.create')],
    loadComponent: () =>
      import('@features/events/create-event/create-event.component').then(
        (m) => m.CreateEventComponent,
      ),
  },
  {
    path: 'reservations',
    canActivate: [authGuard, permissionGuard('reservations.read')],
    loadComponent: () =>
      import('@features/reservations/reservations-list/reservations-list.component').then(
        (m) => m.ReservationsListComponent,
      ),
  },
  { path: '**', redirectTo: '' },
];
