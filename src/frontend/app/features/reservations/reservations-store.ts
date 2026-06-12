import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '@core/api-base-url';
import { CancelReservationResponse, ConfirmPaymentResponse } from '@shared/models/reservation';

@Injectable({ providedIn: 'root' })
export class ReservationsStore {
  private readonly http = inject(HttpClient);
  private readonly apiBase = inject(API_BASE_URL);

  /** Command: confirm a reservation's payment (admin). */
  confirmPayment(id: string) {
    return this.http.post<ConfirmPaymentResponse>(`${this.apiBase}/reservations/${id}/confirm`, {});
  }

  /** Command: cancel a reservation. Returns the resulting status (Cancelled or Lost, RN07). */
  cancel(id: string) {
    return this.http.post<CancelReservationResponse>(
      `${this.apiBase}/reservations/${id}/cancel`,
      {},
    );
  }
}
