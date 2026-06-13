import { computed, inject, signal, WritableSignal } from '@angular/core';
import { TableLazyLoadEvent } from 'primeng/table';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TranslocoService } from '@jsverse/transloco';
import { API_BASE_URL } from '@core/api-base-url';
import { showAppError } from '@core/errors/show-app-error';
import { ReservationsStore } from '@features/reservations/reservations-store';
import { ReservationStatus } from '@shared/enums/reservation-status';
import { ReservationListItem } from '@shared/models/reservation';
import { AppError } from '@shared/models/app-error';

export type TagSeverity = 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast';

/**
 * Shared state and behavior for reservation list views. Subclasses only need to define the
 * `reservations` resource pointing to their specific endpoint, and any view-specific actions.
 */
export abstract class ReservationListBase {
  protected readonly store = inject(ReservationsStore);
  protected readonly apiBase = inject(API_BASE_URL);
  protected readonly messages = inject(MessageService);
  protected readonly transloco = inject(TranslocoService);
  protected readonly confirmation = inject(ConfirmationService);

  protected readonly ReservationStatus = ReservationStatus;

  protected readonly statusOptions = [
    ReservationStatus.PendingPayment,
    ReservationStatus.Confirmed,
    ReservationStatus.Cancelled,
    ReservationStatus.Lost,
    ReservationStatus.Expired,
  ].map((value) => ({
    value,
    label: this.transloco.translate(`enums.reservationStatus.${value}`),
  }));

  protected readonly status = signal<ReservationStatus | null>(null);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly first = computed(() => (this.page() - 1) * this.pageSize());

  protected abstract readonly reservations: { reload(): void };

  protected onFilter<T>(target: WritableSignal<T>, value: T): void {
    target.set(value);
    this.page.set(1);
  }

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows && event.rows > 0 ? event.rows : this.pageSize();
    const first = event.first ?? 0;
    this.pageSize.set(rows);
    this.page.set(Math.floor(first / rows) + 1);
  }

  protected canCancel(status: ReservationStatus): boolean {
    return status === ReservationStatus.PendingPayment || status === ReservationStatus.Confirmed;
  }

  protected cancel(item: ReservationListItem): void {
    this.confirmation.confirm({
      header: this.transloco.translate('labels.reservations.cancel.button'),
      message: this.transloco.translate('labels.reservations.cancel.confirm'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.transloco.translate('labels.reservations.cancel.accept'),
      rejectLabel: this.transloco.translate('labels.reservations.cancel.reject'),
      accept: () => this.doCancel(item),
    });
  }

  protected doCancel(item: ReservationListItem): void {
    this.store.cancel(item.id).subscribe({
      next: (response) => {
        const lost = response.status === ReservationStatus.Lost;
        this.messages.add({
          severity: lost ? 'warn' : 'success',
          detail: this.transloco.translate(
            lost ? 'labels.reservations.cancel.lost' : 'labels.reservations.cancel.success',
          ),
        });
        this.reservations.reload();
      },
      error: (error: AppError) => showAppError(error, this.messages, this.transloco),
    });
  }

  protected statusSeverity(status: ReservationStatus): TagSeverity {
    switch (status) {
      case ReservationStatus.Confirmed:
        return 'success';
      case ReservationStatus.PendingPayment:
        return 'warn';
      case ReservationStatus.Cancelled:
      case ReservationStatus.Lost:
        return 'danger';
      default:
        return 'secondary';
    }
  }
}
