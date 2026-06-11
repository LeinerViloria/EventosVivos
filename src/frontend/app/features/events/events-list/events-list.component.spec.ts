import { render, screen, RenderResult } from '@testing-library/angular';
import { TestBed } from '@angular/core/testing';
import { WritableSignal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';

import { EventsListComponent } from './events-list.component';
import { EventsStore } from '@features/events/events-store';
import { EventType } from '@shared/enums/event-type';
import { EventStatus } from '@shared/enums/event-status';

interface EventsListApi {
  title: WritableSignal<string>;
  type: WritableSignal<EventType | null>;
  status: WritableSignal<EventStatus | null>;
  venueId: WritableSignal<string | null>;
  dateRange: WritableSignal<Date[] | null>;
  page: WritableSignal<number>;
  pageSize: WritableSignal<number>;
  onTitleInput: (value: string) => void;
  onLazyLoad: (event: { first?: number; rows?: number }) => void;
  clearFilters: () => void;
  statusSeverity: (status: EventStatus) => string;
}

function api(view: RenderResult<EventsListComponent>): EventsListApi {
  return view.fixture.componentInstance as unknown as EventsListApi;
}

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

const emptyPage = { items: [], total: 0, page: 1, pageSize: 10 };

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

  it('includes every active filter in the request', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/events')).flush(emptyPage);

    const component = api(view);
    component.type.set(EventType.Concert);
    component.status.set(EventStatus.Active);
    component.venueId.set('venue-1');
    component.dateRange.set([new Date('2026-12-01T00:00:00Z'), new Date('2026-12-31T00:00:00Z')]);
    component.title.set('jazz');
    view.detectChanges();

    const request = controller.expectOne((r) => r.url.includes('/events'));
    expect(request.request.url).toContain('type=3');
    expect(request.request.url).toContain('status=1');
    expect(request.request.url).toContain('venueId=venue-1');
    expect(request.request.url).toContain('startFrom=');
    expect(request.request.url).toContain('startTo=');
    expect(request.request.url).toContain('title=jazz');
    request.flush(emptyPage);
  });

  it('maps lazy-load paging and guards against a zero page size', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/events')).flush(emptyPage);
    const component = api(view);

    component.onLazyLoad({ first: 20, rows: 10 });
    expect(component.page()).toBe(3);
    expect(component.pageSize()).toBe(10);

    component.onLazyLoad({ first: 0, rows: 0 });
    expect(component.pageSize()).toBe(10);
    expect(component.page()).toBe(1);
  });

  it('clears all filters and returns to the first page', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/events')).flush(emptyPage);
    const component = api(view);

    component.title.set('x');
    component.type.set(EventType.Concert);
    component.venueId.set('v');
    component.page.set(5);

    component.clearFilters();

    expect(component.title()).toBe('');
    expect(component.type()).toBeNull();
    expect(component.venueId()).toBeNull();
    expect(component.page()).toBe(1);
  });

  it('maps each status to a tag severity', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/events')).flush(emptyPage);
    const component = api(view);

    expect(component.statusSeverity(EventStatus.Active)).toBe('success');
    expect(component.statusSeverity(EventStatus.Cancelled)).toBe('danger');
    expect(component.statusSeverity(EventStatus.Completed)).toBe('secondary');
    expect(component.statusSeverity(99 as EventStatus)).toBe('info');
  });

  it('debounces the title search before applying it', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/events')).flush(emptyPage);
    const component = api(view);

    component.onTitleInput('rock');
    expect(component.title()).toBe('');

    await new Promise((resolve) => setTimeout(resolve, 450));

    expect(component.title()).toBe('rock');
    expect(component.page()).toBe(1);
  });
});
