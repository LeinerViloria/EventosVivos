using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ListMyReservations;
using EventosVivos.Application.Features.Reservations.ListReservations;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class MyReservationListReader(EventosVivosDbContext context) : IMyReservationListReader
{
    public async Task<PagedResult<ReservationListItem>> ListAsync(
        ListMyReservationsQuery query,
        CancellationToken cancellationToken)
    {
        // The identity token carries only the user id (sub). We resolve the normalized email
        // from the users table so we can match against buyer_email, stored lowercased.
        var email = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == query.UserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (email is null)
        {
            return new PagedResult<ReservationListItem>([], 0, query.Page, query.PageSize);
        }

        var reservations = context.Reservations
            .AsNoTracking()
            .Where(r => r.BuyerEmail == email);

        if (query.Status is not null)
        {
            reservations = reservations.Where(r => r.Status == query.Status);
        }

        return await ReservationPaginatedQuery.ExecuteAsync(
            reservations,
            context.Events.AsNoTracking(),
            query.Page,
            query.PageSize,
            cancellationToken);
    }
}
