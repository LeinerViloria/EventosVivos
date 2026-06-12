using EventosVivos.Application.Abstractions;

namespace EventosVivos.Application.Features.Reservations.ListReservations;

/// <summary>Read model for the reservations listing, implemented with EF Core.</summary>
public interface IReservationListReader
{
    Task<PagedResult<ReservationListItem>> ListAsync(
        ListReservationsQuery query,
        CancellationToken cancellationToken);
}
