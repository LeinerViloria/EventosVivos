namespace EventosVivos.Domain.Reservations;

/// <summary>
/// Reservation lifecycle. Pending, confirmed and lost reservations hold tickets; cancelled and
/// expired ones release them. The contract travels as the number (enum : byte).
/// </summary>
public enum ReservationStatus : byte
{
    PendingPayment = 1,
    Confirmed = 2,
    Cancelled = 3,
    Lost = 4,
    Expired = 5,
}
