import { Component } from '@angular/core';
import { DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TranslocoModule } from '@jsverse/transloco';
import { showAppError } from '@core/errors/show-app-error';
import { ReservationListBase } from '@features/reservations/reservation-list-base';
import { EnumLabelPipe } from '@shared/pipes/enum-label.pipe';
import { ReservationListItem } from '@shared/models/reservation';
import { PagedResult } from '@shared/models/paged-result';
import { AppError } from '@shared/models/app-error';

@Component({
  selector: 'app-reservations-list',
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
  templateUrl: './reservations-list.component.html',
})
export class ReservationsListComponent extends ReservationListBase {
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
}
