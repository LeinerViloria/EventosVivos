using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ListMyReservations;
using EventosVivos.Application.Features.Reservations.ListReservations;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class MyReservationListReader(EventosVivosDbContext context) : IMyReservationListReader
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<ReservationListItem>> ListAsync(
        ListMyReservationsQuery query,
        CancellationToken cancellationToken)
    {
        // The identity token carries only the user id (sub). We resolve the normalized email
        // from the users table so we can match against the reservation's buyer_email column,
        // which is already stored in lowercase (see Reservation.CreatePending).
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

        var rows = from r in reservations
                   join e in context.Events.AsNoTracking() on r.EventId equals e.Id
                   select new { Reservation = r, Event = e };

        var total = await rows.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var items = await rows
            .OrderByDescending(row => row.Reservation.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new ReservationListItem(
                row.Reservation.Id,
                row.Event.Id,
                row.Event.Title,
                row.Reservation.BuyerName,
                row.Reservation.BuyerEmail,
                row.Reservation.Quantity,
                row.Reservation.Status,
                row.Reservation.ConfirmationCode,
                row.Reservation.CreatedAtUtc,
                row.Reservation.ExpiresAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ReservationListItem>(items, total, page, pageSize);
    }
}
