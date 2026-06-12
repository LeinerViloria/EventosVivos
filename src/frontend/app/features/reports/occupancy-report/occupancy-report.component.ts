import { Component, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ProgressBarModule } from 'primeng/progressbar';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslocoModule } from '@jsverse/transloco';
import { TranslocoService } from '@jsverse/transloco';
import { API_BASE_URL } from '@core/api-base-url';
import { showAppError } from '@core/errors/show-app-error';
import { ReportsStore } from '@features/reports/reports-store';
import { EnumLabelPipe } from '@shared/pipes/enum-label.pipe';
import { EventStatus } from '@shared/enums/event-status';
import { OccupancyReportItem } from '@shared/models/occupancy-report';
import { PagedResult } from '@shared/models/paged-result';
import { AppError } from '@shared/models/app-error';

type TagSeverity = 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast';

@Component({
  selector: 'app-occupancy-report',
  imports: [
    TableModule,
    ButtonModule,
    TagModule,
    ProgressBarModule,
    ToastModule,
    TranslocoModule,
    EnumLabelPipe,
    DecimalPipe,
  ],
  providers: [MessageService],
  templateUrl: './occupancy-report.component.html',
})
export class OccupancyReportComponent {
  private readonly store = inject(ReportsStore);
  private readonly apiBase = inject(API_BASE_URL);
  private readonly transloco = inject(TranslocoService);
  private readonly messages = inject(MessageService);

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly downloading = signal(false);

  protected readonly first = computed(() => (this.page() - 1) * this.pageSize());

  protected readonly report = httpResource<PagedResult<OccupancyReportItem>>(
    () => {
      const params = new URLSearchParams();
      params.set('page', String(this.page()));
      params.set('pageSize', String(this.pageSize()));
      return `${this.apiBase}/reports/occupancy?${params.toString()}`;
    },
    { defaultValue: { items: [], total: 0, page: 1, pageSize: 10 } },
  );

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows && event.rows > 0 ? event.rows : this.pageSize();
    const first = event.first ?? 0;
    this.pageSize.set(rows);
    this.page.set(Math.floor(first / rows) + 1);
  }

  protected download(): void {
    this.downloading.set(true);
    this.store.downloadPdf().subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = 'reporte-ocupacion.pdf';
        anchor.click();
        URL.revokeObjectURL(url);
        this.downloading.set(false);
      },
      error: (error: AppError) => {
        showAppError(error, this.messages, this.transloco);
        this.downloading.set(false);
      },
    });
  }

  protected statusSeverity(status: EventStatus): TagSeverity {
    switch (status) {
      case EventStatus.Active:
        return 'success';
      case EventStatus.Completed:
        return 'info';
      case EventStatus.Cancelled:
        return 'danger';
      default:
        return 'secondary';
    }
  }
}
