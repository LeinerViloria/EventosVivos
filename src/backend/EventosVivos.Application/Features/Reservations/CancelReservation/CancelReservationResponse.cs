using EventosVivos.Domain.Reservations;

namespace EventosVivos.Application.Features.Reservations.CancelReservation;

/// <summary>The resulting status after cancelling: <c>Cancelled</c> (tickets released) or <c>Lost</c> (RN07).</summary>
public sealed record CancelReservationResponse(ReservationStatus Status);
