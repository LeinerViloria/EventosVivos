import { render, screen, fireEvent } from '@testing-library/angular';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { CreateEventComponent } from './create-event.component';
import { EventsStore } from '@features/events/events-store';
import { EventType } from '@shared/enums/event-type';
import { VenueSearchItem } from '@shared/models/venue';

const esCO = {
  labels: {
    'nav.createEvent': 'Crear evento',
    'event.subtitle': 'Completa los datos del nuevo evento.',
    'event.submit': 'Crear evento',
    'event.cancel': 'Cancelar',
    'event.created': 'Evento creado correctamente.',
  },
  field: {
    'event.title': 'Título',
    'event.description': 'Descripción',
    'event.venue': 'Lugar',
    'event.capacity': 'Capacidad máxima',
    'event.start': 'Fecha y hora de inicio',
    'event.end': 'Fecha y hora de fin',
    'event.price': 'Precio de entrada',
    'event.type': 'Tipo de evento',
    'event.priceHint': 'Moneda: COP',
    'event.venuePlaceholder': 'Selecciona un lugar',
    'event.typePlaceholder': 'Selecciona un tipo',
  },
  enums: {
    eventType: { '1': 'Conferencia', '2': 'Taller', '3': 'Concierto' },
  },
  errors: {
    required: 'Este campo es obligatorio.',
    EVENT_TITLE_LENGTH: 'El título debe tener entre 5 y 100 caracteres.',
    EVENT_DESCRIPTION_LENGTH: 'La descripción debe tener entre 10 y 500 caracteres.',
    EVENT_VENUE_REQUIRED: 'El lugar es obligatorio.',
  },
};

const venues: VenueSearchItem[] = [
  { id: 'venue-1', name: 'Teatro Colón', capacity: 500, city: 'Bogotá' },
];

function createStoreMock() {
  return {
    venues: { value: () => venues },
    createEvent: vi.fn().mockReturnValue(of({ id: 'evt-1' })),
  };
}

async function setup() {
  const store = createStoreMock();
  const view = await render(CreateEventComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      { provide: EventsStore, useValue: store },
      provideRouter([]),
      providePrimeNG({ theme: { preset: Aura } }),
    ],
  });

  // The component members are `protected`; the cast exposes them for the test.
  const component = view.fixture.componentInstance as unknown as {
    form: {
      setValue: (value: Record<string, unknown>) => void;
      invalid: boolean;
    };
  };

  return { store, view, component };
}

describe('CreateEventComponent', () => {
  it('renders the form with its main fields and the submit button', async () => {
    await setup();

    expect(screen.getByText(/Título/)).toBeTruthy();
    expect(screen.getByText(/Lugar/)).toBeTruthy();
    expect(screen.getByText(/Tipo de evento/)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Crear evento' })).toBeTruthy();
  });

  it('does not submit and shows validation when the form is empty', async () => {
    const { store } = await setup();

    fireEvent.click(screen.getByRole('button', { name: 'Crear evento' }));

    expect(store.createEvent).not.toHaveBeenCalled();
    expect(await screen.findByText('El título debe tener entre 5 y 100 caracteres.')).toBeTruthy();
  });

  it('submits a well-formed request when the form is valid', async () => {
    const { store, component } = await setup();

    component.form.setValue({
      title: '  Concierto de Rock  ',
      description: 'Una gran noche de rock en vivo con bandas locales.',
      venueId: 'venue-1',
      maxCapacity: 100,
      startsAt: new Date(2026, 6, 1, 20, 0),
      endsAt: new Date(2026, 6, 1, 23, 0),
      price: 50000,
      eventType: EventType.Concert,
    });

    fireEvent.click(screen.getByRole('button', { name: 'Crear evento' }));

    expect(store.createEvent).toHaveBeenCalledTimes(1);
    const request = store.createEvent.mock.calls[0][0];
    expect(request).toMatchObject({
      title: 'Concierto de Rock',
      description: 'Una gran noche de rock en vivo con bandas locales.',
      venueId: 'venue-1',
      maxCapacity: 100,
      price: 50000,
      type: EventType.Concert,
    });
    expect(request.startsAt).toMatch(/^2026-07-01T20:00:00[+-]\d{2}:\d{2}$/);
    expect(request.endsAt).toMatch(/^2026-07-01T23:00:00[+-]\d{2}:\d{2}$/);
  });
});
