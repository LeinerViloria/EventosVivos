using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reports.OccupancyReport;
using EventosVivos.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class OccupancyReportReader(EventosVivosDbContext context) : IOccupancyReportReader
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<OccupancyReportItem>> GetAsync(
        OccupancyReportQuery query,
        CancellationToken cancellationToken)
    {
        var events = context.Events.AsNoTracking();

        var total = await events.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        // Confirmed tickets are summed in the database; the rest of the metrics derive from the
        // event's own columns. Nothing is pulled into memory to be aggregated.
        var rows = events
            .OrderBy(e => e.StartUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                Event = e,
                Sold = context.Reservations
                    .Where(r => r.EventId == e.Id && r.Status == ReservationStatus.Confirmed)
                    .Sum(r => (int?)r.Quantity) ?? 0,
            });

        var items = await rows
            .Select(x => new OccupancyReportItem(
                x.Event.Id,
                x.Event.Title,
                x.Event.MaxCapacity,
                x.Sold,
                x.Event.MaxCapacity - x.Event.ReservedTickets,
                x.Event.MaxCapacity == 0
                    ? 0
                    : (double)x.Event.ReservedTickets * 100 / x.Event.MaxCapacity,
                x.Event.Price * x.Sold,
                x.Event.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<OccupancyReportItem>(items, total, page, pageSize);
    }
}
