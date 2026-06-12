import { render, screen, fireEvent, RenderResult } from '@testing-library/angular';
import { TestBed } from '@angular/core/testing';
import { WritableSignal } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { providePrimeNG } from 'primeng/config';
import { Confirmation, ConfirmationService, MessageService } from 'primeng/api';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { of, throwError } from 'rxjs';
import { vi, Mock } from 'vitest';

import { ReservationsListComponent } from './reservations-list.component';
import { ReservationsStore } from '@features/reservations/reservations-store';
import { ReservationStatus } from '@shared/enums/reservation-status';
import { ReservationListItem } from '@shared/models/reservation';

interface ReservationsListApi {
  status: WritableSignal<ReservationStatus | null>;
  page: WritableSignal<number>;
  pageSize: WritableSignal<number>;
  onFilter: (
    target: WritableSignal<ReservationStatus | null>,
    value: ReservationStatus | null,
  ) => void;
  onLazyLoad: (event: { first?: number; rows?: number }) => void;
  statusSeverity: (status: ReservationStatus) => string;
  canCancel: (status: ReservationStatus) => boolean;
  confirm: (item: ReservationListItem) => void;
  cancel: (item: ReservationListItem) => void;
}

function api(view: RenderResult<ReservationsListComponent>): ReservationsListApi {
  return view.fixture.componentInstance as unknown as ReservationsListApi;
}

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
  cancel = vi.fn().mockReturnValue(of({ status: ReservationStatus.Cancelled })),
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
      { provide: ReservationsStore, useValue: { confirmPayment, cancel } },
      provideHttpClient(),
      provideHttpClientTesting(),
      providePrimeNG({ theme: { preset: Aura } }),
    ],
  });
  const controller = TestBed.inject(HttpTestingController);
  // Use the component's real ConfirmationService/MessageService (the dialog and toast subscribe to
  // their observables); intercept confirm() to auto-accept and spy on add() to assert the toast.
  const injector = view.fixture.debugElement.injector;
  const confirmation = injector.get(ConfirmationService);
  vi.spyOn(confirmation, 'confirm').mockImplementation((options: Confirmation) => {
    options.accept?.();
    return confirmation;
  });
  const add: Mock = vi.spyOn(injector.get(MessageService), 'add') as unknown as Mock;
  return { view, controller, confirmPayment, cancel, add };
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

  it('shows an error when confirming fails', async () => {
    const confirmPayment = vi.fn().mockReturnValue(
      throwError(() => ({
        status: 409,
        errorCode: 'RESERVATION_ALREADY_CONFIRMED',
        errorKind: 'business',
        params: null,
        validationErrors: null,
      })),
    );
    const { view, controller } = await setup(confirmPayment);
    controller.expectOne((r) => r.url.includes('/reservations')).flush(pageResponse);
    const component = api(view);

    component.confirm(pageResponse.items[0] as ReservationListItem);

    expect(confirmPayment).toHaveBeenCalledWith('r1');
  });

  it('updates filter and pagination state', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/reservations')).flush(pageResponse);
    const component = api(view);

    component.onFilter(component.status, ReservationStatus.Confirmed);
    expect(component.status()).toBe(ReservationStatus.Confirmed);
    expect(component.page()).toBe(1);

    component.onLazyLoad({ first: 20, rows: 10 });
    expect(component.page()).toBe(3);
    expect(component.pageSize()).toBe(10);

    component.onLazyLoad({ first: 0, rows: 0 });
    expect(component.pageSize()).toBe(10);
  });

  it('cancels a reservation and reports the tickets were released', async () => {
    const cancel = vi.fn().mockReturnValue(of({ status: ReservationStatus.Cancelled }));
    const { view, controller, add } = await setup(undefined, cancel);
    controller.expectOne((r) => r.url.includes('/reservations')).flush(pageResponse);
    const component = api(view);

    component.cancel(pageResponse.items[0] as ReservationListItem);

    expect(cancel).toHaveBeenCalledWith('r1');
    expect(add).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'success', detail: 'Reserva cancelada.' }),
    );
  });

  it('warns when a cancellation within 48h is recorded as lost (RN07)', async () => {
    const cancel = vi.fn().mockReturnValue(of({ status: ReservationStatus.Lost }));
    const { view, controller, add } = await setup(undefined, cancel);
    controller.expectOne((r) => r.url.includes('/reservations')).flush(pageResponse);
    const component = api(view);

    component.cancel(pageResponse.items[0] as ReservationListItem);

    expect(add).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'warn', detail: 'Reserva perdida.' }),
    );
  });

  it('shows an error when cancelling fails', async () => {
    const cancel = vi.fn().mockReturnValue(
      throwError(() => ({
        status: 409,
        errorCode: 'RESERVATION_NOT_CANCELLABLE',
        errorKind: 'business',
        params: null,
        validationErrors: null,
      })),
    );
    const { view, controller } = await setup(undefined, cancel);
    controller.expectOne((r) => r.url.includes('/reservations')).flush(pageResponse);
    const component = api(view);

    component.cancel(pageResponse.items[0] as ReservationListItem);

    expect(cancel).toHaveBeenCalledWith('r1');
  });

  it('allows cancelling only pending and confirmed reservations', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/reservations')).flush(pageResponse);
    const component = api(view);

    expect(component.canCancel(ReservationStatus.PendingPayment)).toBe(true);
    expect(component.canCancel(ReservationStatus.Confirmed)).toBe(true);
    expect(component.canCancel(ReservationStatus.Cancelled)).toBe(false);
    expect(component.canCancel(ReservationStatus.Lost)).toBe(false);
    expect(component.canCancel(ReservationStatus.Expired)).toBe(false);
  });

  it('maps each status to a tag severity', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/reservations')).flush(pageResponse);
    const component = api(view);

    expect(component.statusSeverity(ReservationStatus.Confirmed)).toBe('success');
    expect(component.statusSeverity(ReservationStatus.PendingPayment)).toBe('warn');
    expect(component.statusSeverity(ReservationStatus.Cancelled)).toBe('danger');
    expect(component.statusSeverity(ReservationStatus.Lost)).toBe('danger');
    expect(component.statusSeverity(ReservationStatus.Expired)).toBe('secondary');
  });
});
