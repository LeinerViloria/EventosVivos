import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { timezoneInterceptor } from './timezone.interceptor';

describe('timezoneInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([timezoneInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('adds the X-Timezone header with the client time zone to every request', () => {
    http.get('/api/test').subscribe();

    const request = controller.expectOne('/api/test');
    const expectedZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    expect(request.request.headers.get('X-Timezone')).toBe(expectedZone);

    request.flush({});
  });
});
