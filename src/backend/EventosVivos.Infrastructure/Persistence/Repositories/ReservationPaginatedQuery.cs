using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ListReservations;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

/// <summary>
/// Shared join + pagination logic reused by all reservation list readers.
/// Accepts a pre-filtered <see cref="IQueryable{Reservation}"/> so each reader
/// applies its own WHERE clauses before calling in.
/// </summary>
internal static class ReservationPaginatedQuery
{
    internal const int MaxPageSize = 100;

    internal static async Task<PagedResult<ReservationListItem>> ExecuteAsync(
        IQueryable<Reservation> reservations,
        IQueryable<Event> events,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var rows = from r in reservations
                   join e in events on r.EventId equals e.Id
                   select new { Reservation = r, Event = e };

        var total = await rows.CountAsync(cancellationToken);

        var p = Math.Max(page, 1);
        var ps = Math.Clamp(pageSize, 1, MaxPageSize);

        var items = await rows
            .OrderByDescending(row => row.Reservation.CreatedAtUtc)
            .Skip((p - 1) * ps)
            .Take(ps)
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

        return new PagedResult<ReservationListItem>(items, total, p, ps);
    }
}
