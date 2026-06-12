import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { ReservationsStore } from './reservations-store';

describe('ReservationsStore', () => {
  let store: ReservationsStore;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), ReservationsStore],
    });
    store = TestBed.inject(ReservationsStore);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('posts to confirm a reservation and returns the code', () => {
    let result: { confirmationCode: string } | undefined;

    store.confirmPayment('r1').subscribe((value) => (result = value));

    const request = controller.expectOne('/api/v1/reservations/r1/confirm');
    expect(request.request.method).toBe('POST');
    request.flush({ confirmationCode: 'EV-123456' });

    expect(result?.confirmationCode).toBe('EV-123456');
  });
});
