using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.CancelReservation;

public sealed class CancelReservationHandler(
    IReservationRepository reservations,
    IEventRepository events,
    IClock clock) : IRequestHandler<CancelReservationCommand, Result<CancelReservationResponse>>
{
    public async ValueTask<Result<CancelReservationResponse>> Handle(
        CancelReservationCommand command,
        CancellationToken cancellationToken)
    {
        var reservation = await reservations.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
        {
            return Result.Failure<CancelReservationResponse>(ReservationErrors.NotFound);
        }

        var @event = await events.GetByIdAsync(reservation.EventId, cancellationToken);
        if (@event is null)
        {
            return Result.Failure<CancelReservationResponse>(EventErrors.NotFound);
        }

        var result = reservation.Cancel(@event.StartUtc, clock.UtcNow);
        if (result.IsFailure)
        {
            return Result.Failure<CancelReservationResponse>(result.Error);
        }

        // RN07: a reservation lost within the 48h window keeps its tickets; only a true cancellation
        // releases them back for sale, under the event's xmin optimistic lock.
        if (reservation.ReleasedOnCancel)
        {
            @event.ReleaseTickets(reservation.Quantity);
        }

        return Result.Success(new CancelReservationResponse(reservation.Status));
    }
}
