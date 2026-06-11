/** Mirrors EventStatus (enum : byte) in the backend. The contract travels as the number. */
export enum EventStatus {
  Active = 1,
  Cancelled = 2,
  Completed = 3,
}
