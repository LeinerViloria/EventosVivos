import { Component, computed, inject, signal, WritableSignal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
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
  selector: 'app-my-reservations',
  imports: [
    FormsModule,
    TableModule,
    SelectModule,
    ButtonModule,
    TagModule,
    ToastModule,
    ConfirmDialogModule,
    TranslocoModule,
    EnumLabelPipe,
    DatePipe,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './my-reservations.component.html',
})
export class MyReservationsComponent {
  private readonly store = inject(ReservationsStore);
  private readonly apiBase = inject(API_BASE_URL);
  private readonly transloco = inject(TranslocoService);
  private readonly messages = inject(MessageService);
  private readonly confirmation = inject(ConfirmationService);

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
      return `${this.apiBase}/reservations/mine?${params.toString()}`;
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

  private doCancel(item: ReservationListItem): void {
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
