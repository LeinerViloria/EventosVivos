import { render, screen, fireEvent, RenderResult } from '@testing-library/angular';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { ReserveDialogComponent } from './reserve-dialog.component';
import { EventsStore } from '@features/events/events-store';
import { AuthStore } from '@core/auth/auth-store';
import { AuthUser } from '@shared/models/auth';
import { EventListItem } from '@shared/models/event';

const esCO = {
  labels: {
    'reserve.title': 'Reservar entradas',
    'reserve.available': 'Disponibles',
    'reserve.buyerName': 'Nombre del comprador',
    'reserve.buyerEmail': 'Correo del comprador',
    'reserve.quantity': 'Cantidad',
    'reserve.submit': 'Reservar',
    'reserve.success': 'Reserva creada.',
  },
  errors: {
    RESERVATION_BUYER_NAME_REQUIRED: 'El nombre es obligatorio.',
    RESERVATION_BUYER_EMAIL_INVALID: 'Correo inválido.',
    RESERVATION_QUANTITY_POSITIVE: 'Cantidad inválida.',
  },
};

const anEvent: EventListItem = {
  id: 'e1',
  title: 'Concierto de Rock',
  venueId: 'v1',
  venueName: 'Teatro Colón',
  startUtc: '2026-12-01T18:00:00Z',
  endUtc: '2026-12-01T20:00:00Z',
  maxCapacity: 100,
  reservedTickets: 10,
  price: 50000,
  type: 3,
  status: 1,
};

interface ReserveDialogApi {
  form: {
    setValue: (value: Record<string, unknown>) => void;
    getRawValue: () => Record<string, unknown>;
  };
  onHide: () => void;
}

async function setup(
  createReservation = vi.fn().mockReturnValue(of({ id: 'r1', expiresAtUtc: 'x' })),
  user: AuthUser | null = null,
) {
  const view: RenderResult<ReserveDialogComponent> = await render(ReserveDialogComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      { provide: EventsStore, useValue: { createReservation } },
      { provide: AuthStore, useValue: { user: () => user } },
      providePrimeNG({ theme: { preset: Aura } }),
    ],
    inputs: { event: anEvent },
  });
  const component = view.fixture.componentInstance as unknown as ReserveDialogApi;
  return { view, component, createReservation };
}

describe('ReserveDialogComponent', () => {
  it('shows the event being reserved and the submit button', async () => {
    await setup();

    expect(screen.getByText(/Concierto de Rock/)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Reservar' })).toBeTruthy();
  });

  it('reserves with the entered details', async () => {
    const { component, createReservation } = await setup();

    component.form.setValue({ buyerName: 'Ana', buyerEmail: 'ana@example.com', quantity: 3 });
    fireEvent.click(screen.getByRole('button', { name: 'Reservar' }));

    expect(createReservation).toHaveBeenCalledWith({
      eventId: 'e1',
      buyerName: 'Ana',
      buyerEmail: 'ana@example.com',
      quantity: 3,
    });
  });

  it('prefills the buyer with the signed-in user', async () => {
    const user: AuthUser = {
      name: 'Administrador',
      email: 'admin@eventosvivos.dev',
      role: 'Admin',
      permissions: [],
    };

    const { component } = await setup(undefined, user);

    expect(component.form.getRawValue()['buyerName']).toBe('Administrador');
    expect(component.form.getRawValue()['buyerEmail']).toBe('admin@eventosvivos.dev');
  });

  it('shows the error when the reservation fails', async () => {
    const createReservation = vi.fn().mockReturnValue(
      throwError(() => ({
        status: 409,
        errorCode: 'RESERVATION_NO_TICKETS_AVAILABLE',
        errorKind: 'business',
        params: { available: 0 },
        validationErrors: null,
      })),
    );
    const { component } = await setup(createReservation);

    component.form.setValue({ buyerName: 'Ana', buyerEmail: 'ana@example.com', quantity: 3 });
    fireEvent.click(screen.getByRole('button', { name: 'Reservar' }));

    expect(createReservation).toHaveBeenCalledTimes(1);
  });

  it('resets the form when the dialog is hidden', async () => {
    const { component } = await setup();

    component.form.setValue({ buyerName: 'Ana', buyerEmail: 'ana@example.com', quantity: 5 });
    component.onHide();

    expect(component.form.getRawValue()['buyerName']).toBe('');
    expect(component.form.getRawValue()['quantity']).toBe(1);
  });
});
