import { Component, computed, inject, signal, WritableSignal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { API_BASE_URL } from '@core/api-base-url';
import { showAppError } from '@core/errors/show-app-error';
import { ReservationsStore } from '@features/reservations/reservations-store';
import { EnumLabelPipe } from '@shared/pipes/enum-label.pipe';
import { ReservationStatus } from '@shared/enums/reservation-status';
import { ReservationListItem } from '@shared/models/reservation';
import { PagedResult } from '@shared/models/paged-result';
import { AppError } from '@shared/models/app-error';

type TagSeverity = 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast';

@Component({
  selector: 'app-reservations-list',
  imports: [
    FormsModule,
    TableModule,
    SelectModule,
    ButtonModule,
    TagModule,
    ToastModule,
    TranslocoModule,
    EnumLabelPipe,
    DatePipe,
  ],
  providers: [MessageService],
  templateUrl: './reservations-list.component.html',
})
export class ReservationsListComponent {
  private readonly store = inject(ReservationsStore);
  private readonly apiBase = inject(API_BASE_URL);
  private readonly transloco = inject(TranslocoService);
  private readonly messages = inject(MessageService);

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

  protected readonly reservations = httpResource<PagedResult<ReservationListItem>>(
    () => {
      const params = new URLSearchParams();
      if (this.status() !== null) {
        params.set('status', String(this.status()));
      }
      params.set('page', String(this.page()));
      params.set('pageSize', String(this.pageSize()));
      return `${this.apiBase}/reservations?${params.toString()}`;
    },
    { defaultValue: { items: [], total: 0, page: 1, pageSize: 10 } },
  );

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

  protected confirm(item: ReservationListItem): void {
    this.store.confirmPayment(item.id).subscribe({
      next: (response) => {
        this.messages.add({
          severity: 'success',
          detail: this.transloco.translate('labels.reservations.confirm.success', {
            code: response.confirmationCode,
          }),
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
