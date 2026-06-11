using EventosVivos.Application.Abstractions;
using EventosVivos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Messaging;

/// <summary>
/// Publishes pending outbox messages to the bus, oldest first, and marks them processed. Delivery
/// is at-least-once (a crash after publishing but before marking re-sends), so consumers deduplicate.
/// </summary>
public sealed class OutboxProcessor(EventosVivosDbContext context, IEventBus bus, IClock clock)
{
    private const int BatchSize = 50;

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var pending = await context.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                await bus.PublishAsync(message.Type, message.Payload, cancellationToken);
                message.MarkProcessed(clock.UtcNow.UtcDateTime);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                // Leave it pending; the next tick retries it.
                message.RecordFailure();
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
