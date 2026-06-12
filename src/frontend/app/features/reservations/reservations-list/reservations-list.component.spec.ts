import { render, screen, fireEvent } from '@testing-library/angular';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { ReservationsListComponent } from './reservations-list.component';
import { ReservationsStore } from '@features/reservations/reservations-store';

const esCO = {
  labels: {
    'reservations.title': 'Reservas',
    'reservations.subtitle': 'Gestiona y confirma las reservas.',
    'reservations.filter.status': 'Estado',
    'reservations.empty': 'Sin reservas',
    'reservations.column.event': 'Evento',
    'reservations.column.buyer': 'Comprador',
    'reservations.column.quantity': 'Cantidad',
    'reservations.column.status': 'Estado',
    'reservations.column.code': 'Código',
    'reservations.column.created': 'Creada',
    'reservations.column.actions': 'Acciones',
    'reservations.confirm.button': 'Confirmar pago',
    'reservations.confirm.success': 'Pago confirmado.',
  },
  enums: {
    reservationStatus: {
      '1': 'Pendiente de pago',
      '2': 'Confirmada',
      '3': 'Cancelada',
      '4': 'Perdida',
      '5': 'Expirada',
    },
  },
};

const pageResponse = {
  items: [
    {
      id: 'r1',
      eventId: 'e1',
      eventTitle: 'Concierto de Rock',
      buyerName: 'Ana',
      buyerEmail: 'ana@example.com',
      quantity: 2,
      status: 1,
      confirmationCode: null,
      createdAtUtc: '2026-12-01T18:00:00Z',
      expiresAtUtc: '2026-12-01T18:15:00Z',
    },
  ],
  total: 1,
  page: 1,
  pageSize: 10,
};

async function setup(
  confirmPayment = vi.fn().mockReturnValue(of({ confirmationCode: 'EV-123456' })),
) {
  const view = await render(ReservationsListComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      { provide: ReservationsStore, useValue: { confirmPayment } },
      provideHttpClient(),
      provideHttpClientTesting(),
      providePrimeNG({ theme: { preset: Aura } }),
    ],
  });
  const controller = TestBed.inject(HttpTestingController);
  return { view, controller, confirmPayment };
}

describe('ReservationsListComponent', () => {
  it('requests reservations and renders a row', async () => {
    const { view, controller } = await setup();

    const request = controller.expectOne((r) => r.url.includes('/reservations'));
    expect(request.request.url).toContain('page=1');
    request.flush(pageResponse);

    await view.fixture.whenStable();
    view.detectChanges();

    expect(screen.getByText('Concierto de Rock')).toBeTruthy();
    expect(screen.getByText('Pendiente de pago')).toBeTruthy();
  });

  it('confirms a pending reservation', async () => {
    const { view, controller, confirmPayment } = await setup();
    controller.expectOne((r) => r.url.includes('/reservations')).flush(pageResponse);

    await view.fixture.whenStable();
    view.detectChanges();

    fireEvent.click(screen.getByRole('button', { name: 'Confirmar pago' }));

    expect(confirmPayment).toHaveBeenCalledWith('r1');
  });
});
