using EventosVivos.Application.Abstractions;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.ListReservations;

public sealed class ListReservationsHandler(IReservationListReader reader)
    : IRequestHandler<ListReservationsQuery, PagedResult<ReservationListItem>>
{
    public ValueTask<PagedResult<ReservationListItem>> Handle(
        ListReservationsQuery query,
        CancellationToken cancellationToken) =>
        new(reader.ListAsync(query, cancellationToken));
}
