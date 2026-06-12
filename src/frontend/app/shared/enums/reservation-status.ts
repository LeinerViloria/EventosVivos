/** Mirrors ReservationStatus (enum : byte) in the backend. The contract travels as the number. */
export enum ReservationStatus {
  PendingPayment = 1,
  Confirmed = 2,
  Cancelled = 3,
  Lost = 4,
  Expired = 5,
}
