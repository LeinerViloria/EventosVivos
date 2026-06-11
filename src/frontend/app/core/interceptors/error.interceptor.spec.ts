import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { errorInterceptor } from './error.interceptor';
import { AppError } from '@shared/models/app-error';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('maps a ProblemDetails business error into a typed AppError', () => {
    let captured: AppError | undefined;
    http.post('/api/events', {}).subscribe({ error: (error: AppError) => (captured = error) });

    controller
      .expectOne('/api/events')
      .flush(
        { errorCode: 'VENUE_NOT_FOUND', errorKind: 'business', params: { id: '1' } },
        { status: 409, statusText: 'Conflict' },
      );

    expect(captured).toEqual({
      status: 409,
      errorCode: 'VENUE_NOT_FOUND',
      errorKind: 'business',
      params: { id: '1' },
      validationErrors: null,
    });
  });

  it('keeps the per-field codes of a 422 validation error', () => {
    let captured: AppError | undefined;
    http.post('/api/events', {}).subscribe({ error: (error: AppError) => (captured = error) });

    const errors = [{ field: 'EndsAt', errorCode: 'EVENT_END_AFTER_START', params: null }];
    controller
      .expectOne('/api/events')
      .flush(
        { errorKind: 'validation', errors },
        { status: 422, statusText: 'Unprocessable Entity' },
      );

    expect(captured?.status).toBe(422);
    expect(captured?.errorKind).toBe('validation');
    expect(captured?.validationErrors).toEqual(errors);
    expect(captured?.errorCode).toBe('UNKNOWN_ERROR');
  });

  it('falls back to UNKNOWN_ERROR when the response has no problem body', () => {
    let captured: AppError | undefined;
    http.get('/api/x').subscribe({ error: (error: AppError) => (captured = error) });

    controller.expectOne('/api/x').flush(null, { status: 500, statusText: 'Server Error' });

    expect(captured?.errorCode).toBe('UNKNOWN_ERROR');
    expect(captured?.errorKind).toBe('general');
    expect(captured?.params).toBeNull();
    expect(captured?.validationErrors).toBeNull();
  });
});
