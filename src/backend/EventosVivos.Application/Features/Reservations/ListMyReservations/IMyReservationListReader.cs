using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ListReservations;

namespace EventosVivos.Application.Features.Reservations.ListMyReservations;

/// <summary>Read model for the user's own reservations, implemented with EF Core.</summary>
public interface IMyReservationListReader
{
    Task<PagedResult<ReservationListItem>> ListAsync(
        ListMyReservationsQuery query,
        CancellationToken cancellationToken);
}
