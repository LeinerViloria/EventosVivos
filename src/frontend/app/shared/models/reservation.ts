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
