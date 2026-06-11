using EventosVivos.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository(EventosVivosDbContext context) : IEventRepository
{
    public void Add(Event @event) => context.Events.Add(@event);

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> HasOverlappingActiveEventAsync(
        Guid venueId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken) =>
        context.Events.AnyAsync(
            e => e.VenueId == venueId
                && e.Status == EventStatus.Active
                && e.StartUtc < endUtc
                && e.EndUtc > startUtc,
            cancellationToken);
}
