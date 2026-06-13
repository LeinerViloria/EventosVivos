using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ListReservations;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class ReservationListReader(EventosVivosDbContext context) : IReservationListReader
{
    public Task<PagedResult<ReservationListItem>> ListAsync(
        ListReservationsQuery query,
        CancellationToken cancellationToken)
    {
        var reservations = context.Reservations.AsNoTracking();

        if (query.Status is not null)
        {
            reservations = reservations.Where(r => r.Status == query.Status);
        }

        return ReservationPaginatedQuery.ExecuteAsync(
            reservations,
            context.Events.AsNoTracking(),
            query.Page,
            query.PageSize,
            cancellationToken);
    }
}
