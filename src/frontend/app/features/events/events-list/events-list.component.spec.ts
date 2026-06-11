import { render, screen } from '@testing-library/angular';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';

import { EventsListComponent } from './events-list.component';
import { EventsStore } from '@features/events/events-store';

const esCO = {
  labels: {
    'events.title': 'Eventos',
    'events.subtitle': 'Consulta y filtra los eventos.',
    'events.search': 'Buscar por título',
    'events.filter.type': 'Tipo',
    'events.filter.status': 'Estado',
    'events.filter.venue': 'Lugar',
    'events.filter.dateRange': 'Rango',
    'events.clear': 'Limpiar',
    'events.empty': 'Sin resultados',
    'events.column.title': 'Título',
    'events.column.venue': 'Lugar',
    'events.column.start': 'Inicio',
    'events.column.end': 'Fin',
    'events.column.type': 'Tipo',
    'events.column.status': 'Estado',
    'events.column.capacity': 'Capacidad',
    'events.column.price': 'Precio',
  },
  enums: {
    eventType: { '1': 'Conferencia', '2': 'Taller', '3': 'Concierto' },
    eventStatus: { '1': 'Activo', '2': 'Cancelado', '3': 'Completado' },
  },
};

const pageResponse = {
  items: [
    {
      id: 'e1',
      title: 'Jazz Night',
      venueId: 'v1',
      venueName: 'Teatro Colón',
      startUtc: '2026-12-01T18:00:00Z',
      endUtc: '2026-12-01T20:00:00Z',
      maxCapacity: 100,
      reservedTickets: 10,
      price: 50000,
      type: 3,
      status: 1,
    },
  ],
  total: 1,
  page: 1,
  pageSize: 10,
};

async function setup() {
  const view = await render(EventsListComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      { provide: EventsStore, useValue: { venues: { value: () => [] } } },
      provideRouter([]),
      provideHttpClient(),
      provideHttpClientTesting(),
      providePrimeNG({ theme: { preset: Aura } }),
    ],
  });
  const controller = TestBed.inject(HttpTestingController);
  return { view, controller };
}

describe('EventsListComponent', () => {
  it('requests the first page and renders the returned events', async () => {
    const { view, controller } = await setup();

    const request = controller.expectOne((r) => r.url.includes('/events'));
    expect(request.request.url).toContain('page=1');
    expect(request.request.url).toContain('pageSize=10');
    request.flush(pageResponse);

    await view.fixture.whenStable();
    view.detectChanges();

    expect(screen.getByText('Eventos')).toBeTruthy();
    expect(await screen.findByText('Jazz Night')).toBeTruthy();
    expect(screen.getByText('Teatro Colón')).toBeTruthy();
    expect(screen.getByText('Concierto')).toBeTruthy();
  });
});
