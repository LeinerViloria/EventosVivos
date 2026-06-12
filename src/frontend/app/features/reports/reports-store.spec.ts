import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { ReportsStore } from './reports-store';

describe('ReportsStore', () => {
  let store: ReportsStore;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), ReportsStore],
    });
    store = TestBed.inject(ReportsStore);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('downloads the occupancy report as a blob', () => {
    let result: Blob | undefined;

    store.downloadPdf().subscribe((value) => (result = value));

    const request = controller.expectOne('/api/v1/reports/occupancy/pdf');
    expect(request.request.method).toBe('GET');
    expect(request.request.responseType).toBe('blob');
    const blob = new Blob(['%PDF-1.7'], { type: 'application/pdf' });
    request.flush(blob);

    expect(result).toBeInstanceOf(Blob);
  });
});
