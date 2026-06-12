using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reports.OccupancyReport;
using EventosVivos.Domain.Events;
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
        var total = await context.Events.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var items = await Project()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<OccupancyReportItem>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<OccupancyReportItem>> GetAllAsync(CancellationToken cancellationToken) =>
        await Project().ToListAsync(cancellationToken);

    /// <summary>
    /// Per-event occupancy metrics, ordered by start date. Confirmed tickets are summed in the
    /// database and the rest derive from the event's own columns; nothing is aggregated in memory.
    /// </summary>
    private IQueryable<OccupancyReportItem> Project() =>
        context.Events
            .AsNoTracking()
            .OrderBy(e => e.StartUtc)
            .Select(e => new
            {
                Event = e,
                Sold = context.Reservations
                    .Where(r => r.EventId == e.Id && r.Status == ReservationStatus.Confirmed)
                    .Sum(r => (int?)r.Quantity) ?? 0,
            })
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
                x.Event.Status));
}
