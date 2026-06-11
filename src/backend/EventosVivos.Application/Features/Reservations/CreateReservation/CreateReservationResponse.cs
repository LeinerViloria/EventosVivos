namespace EventosVivos.Application.Features.Reservations.CreateReservation;

public sealed record CreateReservationResponse(Guid Id, DateTime ExpiresAtUtc);
