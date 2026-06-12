using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Events;
using EventosVivos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Messaging;

/// <summary>
/// RN06: marks active events whose end time has passed as completed. Runs on the sweep timer; the
/// event's <c>xmin</c> guards the update, so a conflict rolls the batch back to the next tick.
/// </summary>
public sealed class EventCompletionProcessor(EventosVivosDbContext context, IClock clock)
{
    private const int BatchSize = 100;

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var now = clock.UtcNow.UtcDateTime;

        var overdue = await context.Events
            .Where(e => e.Status == EventStatus.Active && e.EndUtc <= now)
            .OrderBy(e => e.EndUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (overdue.Count == 0)
        {
            return;
        }

        foreach (var @event in overdue)
        {
            @event.Complete(clock.UtcNow);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
