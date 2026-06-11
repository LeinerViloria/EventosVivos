using EventosVivos.Domain.Common;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.CreateReservation;

public sealed record CreateReservationCommand(
    Guid EventId,
    string BuyerName,
    string BuyerEmail,
    int Quantity) : IRequest<Result<CreateReservationResponse>>;
