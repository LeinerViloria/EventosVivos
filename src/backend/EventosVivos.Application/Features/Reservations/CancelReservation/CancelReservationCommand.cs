using EventosVivos.Domain.Common;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.CancelReservation;

public sealed record CancelReservationCommand(Guid ReservationId) : IRequest<Result<CancelReservationResponse>>;
