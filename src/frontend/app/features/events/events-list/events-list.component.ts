import { Component, computed, inject, signal, WritableSignal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { API_BASE_URL } from '@core/api-base-url';
import { EventsStore } from '@features/events/events-store';
import { EnumLabelPipe } from '@shared/pipes/enum-label.pipe';
import { EventType } from '@shared/enums/event-type';
import { EventStatus } from '@shared/enums/event-status';
import { EventListItem } from '@shared/models/event';
import { PagedResult } from '@shared/models/paged-result';

type TagSeverity = 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast';

@Component({
  selector: 'app-events-list',
  imports: [
    FormsModule,
    TableModule,
    SelectModule,
    InputTextModule,
    DatePickerModule,
    ButtonModule,
    TagModule,
    TranslocoModule,
    EnumLabelPipe,
    DatePipe,
    CurrencyPipe,
  ],
  templateUrl: './events-list.component.html',
})
export class EventsListComponent {
  private readonly store = inject(EventsStore);
  private readonly apiBase = inject(API_BASE_URL);
  private readonly transloco = inject(TranslocoService);

  protected readonly venues = this.store.venues;

  protected readonly typeOptions = [
    EventType.Conference,
    EventType.Workshop,
    EventType.Concert,
  ].map((value) => ({ value, label: this.transloco.translate(`enums.eventType.${value}`) }));

  protected readonly statusOptions = [
    EventStatus.Active,
    EventStatus.Cancelled,
    EventStatus.Completed,
  ].map((value) => ({ value, label: this.transloco.translate(`enums.eventStatus.${value}`) }));

  // Filters and pagination state.
  protected readonly title = signal('');
  protected readonly type = signal<EventType | null>(null);
  protected readonly status = signal<EventStatus | null>(null);
  protected readonly venueId = signal<string | null>(null);
  protected readonly dateRange = signal<Date[] | null>(null);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);

  protected readonly first = computed(() => (this.page() - 1) * this.pageSize());

  protected readonly events = httpResource<PagedResult<EventListItem>>(
    () => {
      const params = new URLSearchParams();
      const title = this.title().trim();
      if (title) {
        params.set('title', title);
      }
      if (this.type() !== null) {
        params.set('type', String(this.type()));
      }
      if (this.status() !== null) {
        params.set('status', String(this.status()));
      }
      if (this.venueId()) {
        params.set('venueId', this.venueId()!);
      }
      const range = this.dateRange();
      if (range?.[0]) {
        params.set('startFrom', range[0].toISOString());
      }
      if (range?.[1]) {
        params.set('startTo', range[1].toISOString());
      }
      params.set('page', String(this.page()));
      params.set('pageSize', String(this.pageSize()));
      return `${this.apiBase}/events?${params.toString()}`;
    },
    { defaultValue: { items: [], total: 0, page: 1, pageSize: 10 } },
  );

  private titleTimer?: ReturnType<typeof setTimeout>;

  /** Debounces the title search so it does not fire a request on every keystroke. */
  protected onTitleInput(value: string): void {
    clearTimeout(this.titleTimer);
    this.titleTimer = setTimeout(() => {
      this.page.set(1);
      this.title.set(value);
    }, 400);
  }

  /** Updates a filter and returns to the first page so the result stays consistent. */
  protected onFilter<T>(target: WritableSignal<T>, value: T): void {
    target.set(value);
    this.page.set(1);
  }

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? this.pageSize();
    const first = event.first ?? 0;
    this.pageSize.set(rows);
    this.page.set(Math.floor(first / rows) + 1);
  }

  protected clearFilters(): void {
    this.title.set('');
    this.type.set(null);
    this.status.set(null);
    this.venueId.set(null);
    this.dateRange.set(null);
    this.page.set(1);
  }

  protected statusSeverity(status: EventStatus): TagSeverity {
    switch (status) {
      case EventStatus.Active:
        return 'success';
      case EventStatus.Cancelled:
        return 'danger';
      case EventStatus.Completed:
        return 'secondary';
      default:
        return 'info';
    }
  }
}
