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
import { ReservationListBase } from '@features/reservations/reservation-list-base';
import { EnumLabelPipe } from '@shared/pipes/enum-label.pipe';
import { PagedResult } from '@shared/models/paged-result';
import { ReservationListItem } from '@shared/models/reservation';

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
export class MyReservationsComponent extends ReservationListBase {
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
}
