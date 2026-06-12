import { render, screen, RenderResult } from '@testing-library/angular';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { OccupancyReportComponent } from './occupancy-report.component';
import { ReportsStore } from '@features/reports/reports-store';
import { EventStatus } from '@shared/enums/event-status';

interface OccupancyReportApi {
  page: () => number;
  pageSize: () => number;
  downloading: () => boolean;
  onLazyLoad: (event: { first?: number; rows?: number }) => void;
  download: () => void;
  statusSeverity: (status: EventStatus) => string;
}

function api(view: RenderResult<OccupancyReportComponent>): OccupancyReportApi {
  return view.fixture.componentInstance as unknown as OccupancyReportApi;
}

const esCO = {
  labels: {
    'reports.title': 'Reporte de ocupación',
    'reports.subtitle': 'Ocupación e ingresos por evento.',
    'reports.download': 'Descargar PDF',
    'reports.empty': 'Sin eventos',
    'reports.column.event': 'Evento',
    'reports.column.capacity': 'Capacidad',
    'reports.column.sold': 'Vendidas',
    'reports.column.available': 'Disponibles',
    'reports.column.occupancy': 'Ocupación',
    'reports.column.revenue': 'Ingresos',
    'reports.column.status': 'Estado',
  },
  enums: {
    eventStatus: { '1': 'Activo', '2': 'Cancelado', '3': 'Completado' },
  },
};

const pageResponse = {
  items: [
    {
      eventId: 'e1',
      eventTitle: 'Concierto de Rock',
      capacity: 100,
      soldTickets: 30,
      availableTickets: 60,
      occupancyPercent: 40,
      revenue: 1500,
      status: 1,
    },
  ],
  total: 1,
  page: 1,
  pageSize: 10,
};

async function setup(downloadPdf = vi.fn().mockReturnValue(of(new Blob(['%PDF'])))) {
  const view = await render(OccupancyReportComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      { provide: ReportsStore, useValue: { downloadPdf } },
      provideHttpClient(),
      provideHttpClientTesting(),
      providePrimeNG({ theme: { preset: Aura } }),
    ],
  });
  const controller = TestBed.inject(HttpTestingController);
  return { view, controller, downloadPdf };
}

describe('OccupancyReportComponent', () => {
  it('requests the report and renders a row', async () => {
    const { view, controller } = await setup();

    const request = controller.expectOne((r) => r.url.includes('/reports/occupancy'));
    expect(request.request.url).toContain('page=1');
    request.flush(pageResponse);

    await view.fixture.whenStable();
    view.detectChanges();

    expect(screen.getByText('Concierto de Rock')).toBeTruthy();
    expect(screen.getByText('Activo')).toBeTruthy();
  });

  it('updates pagination state on lazy load', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/reports/occupancy')).flush(pageResponse);
    const component = api(view);

    component.onLazyLoad({ first: 20, rows: 10 });
    expect(component.page()).toBe(3);
    expect(component.pageSize()).toBe(10);

    component.onLazyLoad({ first: 0, rows: 0 });
    expect(component.pageSize()).toBe(10);
  });

  it('downloads the PDF and triggers a file download', async () => {
    const createObjectURL = vi.fn(() => 'blob:mock');
    const revokeObjectURL = vi.fn();
    globalThis.URL.createObjectURL = createObjectURL;
    globalThis.URL.revokeObjectURL = revokeObjectURL;
    const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {});

    const { view, controller, downloadPdf } = await setup();
    controller.expectOne((r) => r.url.includes('/reports/occupancy')).flush(pageResponse);
    const component = api(view);

    component.download();

    expect(downloadPdf).toHaveBeenCalled();
    expect(createObjectURL).toHaveBeenCalled();
    expect(click).toHaveBeenCalled();
    expect(revokeObjectURL).toHaveBeenCalled();
    expect(component.downloading()).toBe(false);

    click.mockRestore();
  });

  it('clears the loading flag when the download fails', async () => {
    const downloadPdf = vi.fn().mockReturnValue(
      throwError(() => ({
        status: 500,
        errorCode: 'INTERNAL_ERROR',
        errorKind: 'general',
        params: null,
        validationErrors: null,
      })),
    );
    const { view, controller } = await setup(downloadPdf);
    controller.expectOne((r) => r.url.includes('/reports/occupancy')).flush(pageResponse);
    const component = api(view);

    component.download();

    expect(downloadPdf).toHaveBeenCalled();
    expect(component.downloading()).toBe(false);
  });

  it('maps each event status to a tag severity', async () => {
    const { view, controller } = await setup();
    controller.expectOne((r) => r.url.includes('/reports/occupancy')).flush(pageResponse);
    const component = api(view);

    expect(component.statusSeverity(EventStatus.Active)).toBe('success');
    expect(component.statusSeverity(EventStatus.Completed)).toBe('info');
    expect(component.statusSeverity(EventStatus.Cancelled)).toBe('danger');
  });
});
