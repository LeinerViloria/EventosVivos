import { render, screen, RenderResult } from '@testing-library/angular';
import { TestBed } from '@angular/core/testing';
import { WritableSignal } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { providePrimeNG } from 'primeng/config';
import { Confirmation, ConfirmationService, MessageService } from 'primeng/api';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { of } from 'rxjs';
import { vi, Mock } from 'vitest';

import { MyReservationsComponent } from './my-reservations.component';
import { ReservationsStore } from '@features/reservations/reservations-store';
import { ReservationStatus } from '@shared/enums/reservation-status';
import { ReservationListItem } from '@shared/models/reservation';

interface MyReservationsApi {
  status: WritableSignal<ReservationStatus | null>;
  page: WritableSignal<number>;
  pageSize: WritableSignal<number>;
  onFilter: (target: WritableSignal<ReservationStatus | null>, value: ReservationStatus | null) => void;
  onLazyLoad: (event: { first?: number; rows?: number }) => void;
  statusSeverity: (status: ReservationStatus) => string;
  canCancel: (status: ReservationStatus) => boolean;
  cancel: (item: ReservationListItem) => void;
}

function api(view: RenderResult<MyReservationsComponent>): MyReservationsApi {
  return view.fixture.componentInstance as unknown as MyReservationsApi;
}

const esCO = {
  labels: {
    'myReservations.title': 'Mis reservas',
    'myReservations.subtitle': 'Historial de reservas realizadas con tu cuenta.',
    'myReservations.filter.status': 'Estado',
    'myReservations.empty': 'Sin reservas',
    'myReservations.column.event': 'Evento',
    'myReservations.column.quantity': 'Cantidad',
    'myReservations.column.status': 'Estado',
    'myReservations.column.code': 'Código',
    'myReservations.column.created': 'Creada',
    'myReservations.column.actions': 'Acciones',
    'reservations.cancel.button': 'Cancelar',
    'reservations.cancel.confirm': '¿Seguro?',
    'reservations.cancel.accept': 'Sí, cancelar',
    'reservations.cancel.reject': 'No',
    'reservations.cancel.success': 'Reserva cancelada.',
    'reservations.cancel.lost': 'Reserva perdida.',
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
      eventTitle: 'Taller de Angular',
      buyerName: 'María',
      buyerEmail: 'maria@example.com',
      quantity: 1,
      status: ReservationStatus.PendingPayment,
      confirmationCode: null,
      createdAtUtc: '2026-12-01T18:00:00Z',
      expiresAtUtc: '2026-12-01T18:15:00Z',
    },
  ],
  total: 1,
  page: 1,
  pageSize: 10,
};

async function setup(cancel = vi.fn().mockReturnValue(of({ status: ReservationStatus.Cancelled }))) {
  const view = await render(MyReservationsComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      { provide: ReservationsStore, useValue: { cancel } },
      provideHttpClient(),
      provideHttpClientTesting(),
      providePrimeNG({ theme: { preset: Aura } }),
    ],
  });
  const controller = TestBed.inject(HttpTestingController);
  const injector = view.fixture.debugElement.injector;
  const confirmation = injector.get(ConfirmationService);
  vi.spyOn(confirmation, 'confirm').mockImplementation((options: Confirmation) => {
    options.accept?.();
    return confirmation;
  });
  const add: Mock = vi.spyOn(injector.get(MessageService), 'add') as unknown as Mock;
  return { view, controller, cancel, add };
}

describe('MyReservationsComponent', () => {
  it('requests reservations from /mine and renders a row', async () => {
    const { view, controller } = await setup();

    const request = controller.expectOne((r) => r.url.includes('/reservations/mine'));
    expect(request.request.url).toContain('page=1');
    request.flush(pageResponse);

    await view.fixture.whenStable();
    view.detectChanges();

    expect(screen.getByText('Taller de Angular')).toBeTruthy();
    expect(screen.getByText('Pendiente de pago')).toBeTruthy();
  });

  it('cancels a reservation and shows success toast', async () => {
    const { view, controller, cancel, add } = await setup();
    controller.expectOne((r) => r.url.includes('/reservations/mine')).flush(pageResponse);
    const component = api(view);

    component.cancel(pageResponse.items[0] as ReservationListItem);

    expect(cancel).toHaveBeenCalledWith('r1');
    expect(add).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'success', detail: 'Reserva cancelada.' }),
    );
  });

  it('warns when cancellation within 48h is recorded as lost (RN07)', async () => {
    const cancel = vi.fn().mockReturnValue(of({ status: ReservationStatus.Lost }));
    const { view, controller, add } = await setup(cancel);
    controller.expectOne((r) => r.url.includes('/reservations/mine')).flush(pageResponse);
    const component = api(view);

    component.cancel(pageResponse.items[0] as ReservationListItem);

    expect(add).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'warn', detail: 'Reserva perdida.' }),
    );
  });

  it('allows cancelling only pending and confirmed reservations', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/reservations/mine')).flush(pageResponse);
    const component = api(view);

    expect(component.canCancel(ReservationStatus.PendingPayment)).toBe(true);
    expect(component.canCancel(ReservationStatus.Confirmed)).toBe(true);
    expect(component.canCancel(ReservationStatus.Cancelled)).toBe(false);
    expect(component.canCancel(ReservationStatus.Lost)).toBe(false);
    expect(component.canCancel(ReservationStatus.Expired)).toBe(false);
  });

  it('updates filter and pagination state', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/reservations/mine')).flush(pageResponse);
    const component = api(view);

    component.onFilter(component.status, ReservationStatus.Confirmed);
    expect(component.status()).toBe(ReservationStatus.Confirmed);
    expect(component.page()).toBe(1);

    component.onLazyLoad({ first: 20, rows: 10 });
    expect(component.page()).toBe(3);
    expect(component.pageSize()).toBe(10);
  });

  it('maps each status to a tag severity', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/reservations/mine')).flush(pageResponse);
    const component = api(view);

    expect(component.statusSeverity(ReservationStatus.Confirmed)).toBe('success');
    expect(component.statusSeverity(ReservationStatus.PendingPayment)).toBe('warn');
    expect(component.statusSeverity(ReservationStatus.Cancelled)).toBe('danger');
    expect(component.statusSeverity(ReservationStatus.Lost)).toBe('danger');
    expect(component.statusSeverity(ReservationStatus.Expired)).toBe('secondary');
  });
});
