using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ListReservations;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.ListMyReservations;

public sealed class ListMyReservationsHandler(IMyReservationListReader reader)
    : IRequestHandler<ListMyReservationsQuery, PagedResult<ReservationListItem>>
{
    public ValueTask<PagedResult<ReservationListItem>> Handle(
        ListMyReservationsQuery query,
        CancellationToken cancellationToken) =>
        new(reader.ListAsync(query, cancellationToken));
}
