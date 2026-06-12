import { ReservationStatus } from '../enums/reservation-status';

/** Payload for POST /api/v1/reservations. */
export interface CreateReservationRequest {
  eventId: string;
  buyerName: string;
  buyerEmail: string;
  quantity: number;
}

export interface CreateReservationResponse {
  id: string;
  expiresAtUtc: string;
}

/** A row of the reservations listing (GET /api/v1/reservations, admin). Dates arrive as UTC ISO. */
export interface ReservationListItem {
  id: string;
  eventId: string;
  eventTitle: string;
  buyerName: string;
  buyerEmail: string;
  quantity: number;
  status: ReservationStatus;
  confirmationCode: string | null;
  createdAtUtc: string;
  expiresAtUtc: string;
}

export interface ConfirmPaymentResponse {
  confirmationCode: string;
}

/** Result of POST /api/v1/reservations/{id}/cancel: Cancelled (tickets released) or Lost (RN07). */
export interface CancelReservationResponse {
  status: ReservationStatus;
}
