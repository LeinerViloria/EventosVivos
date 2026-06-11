using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.CreateReservation;

public sealed class CreateReservationHandler(
    IEventRepository events,
    IReservationRepository reservations,
    ReservationOptions options,
    IClock clock) : IRequestHandler<CreateReservationCommand, Result<CreateReservationResponse>>
{
    public async ValueTask<Result<CreateReservationResponse>> Handle(
        CreateReservationCommand command,
        CancellationToken cancellationToken)
    {
        var @event = await events.GetByIdAsync(command.EventId, cancellationToken);
        if (@event is null)
        {
            return Result.Failure<CreateReservationResponse>(EventErrors.NotFound);
        }

        // Holds the tickets on the event counter (protected by xmin); the TransactionBehavior
        // retries on a concurrency conflict so two buyers cannot oversell the same tickets.
        var reserve = @event.Reserve(command.Quantity, clock.UtcNow);
        if (reserve.IsFailure)
        {
            return Result.Failure<CreateReservationResponse>(reserve.Error);
        }

        var reservation = Reservation.CreatePending(
            @event.Id,
            command.BuyerName,
            command.BuyerEmail,
            command.Quantity,
            clock.UtcNow,
            TimeSpan.FromMinutes(options.TtlMinutes));

        reservations.Add(reservation);

        return Result.Success(new CreateReservationResponse(reservation.Id, reservation.ExpiresAtUtc));
    }
}
