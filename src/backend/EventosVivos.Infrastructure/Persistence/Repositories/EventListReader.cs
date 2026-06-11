using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Events.ListEvents;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class EventListReader(EventosVivosDbContext context) : IEventListReader
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<EventListItem>> ListAsync(
        ListEventsQuery query,
        CancellationToken cancellationToken)
    {
        var events = context.Events.AsNoTracking();

        if (query.Type is not null)
        {
            events = events.Where(e => e.Type == query.Type);
        }

        if (query.Status is not null)
        {
            events = events.Where(e => e.Status == query.Status);
        }

        if (query.VenueId is not null)
        {
            events = events.Where(e => e.VenueId == query.VenueId);
        }

        if (query.StartFrom is not null)
        {
            var from = query.StartFrom.Value.UtcDateTime;
            events = events.Where(e => e.StartUtc >= from);
        }

        if (query.StartTo is not null)
        {
            var to = query.StartTo.Value.UtcDateTime;
            events = events.Where(e => e.StartUtc <= to);
        }

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            events = events.Where(e => EF.Functions.ILike(e.Title, $"%{query.Title}%"));
        }

        var rows = from e in events
                   join v in context.Venues.AsNoTracking() on e.VenueId equals v.Id
                   select new { Event = e, Venue = v };

        var total = await rows.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var items = await rows
            .OrderBy(row => row.Event.StartUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new EventListItem(
                row.Event.Id, row.Event.Title, row.Venue.Id, row.Venue.Name,
                row.Event.StartUtc, row.Event.EndUtc, row.Event.MaxCapacity,
                row.Event.ReservedTickets, row.Event.Price, row.Event.Type, row.Event.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<EventListItem>(items, total, page, pageSize);
    }
}
