using System.Text.Json;
using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Reservations;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Messaging;

/// <summary>
/// Expires pending-payment reservations whose hold has lapsed and releases their tickets. The state
/// change and the <c>TicketsReleased</c> outbox message commit in the same transaction; the event's
/// <c>xmin</c> guards the counter, so a conflict rolls the batch back to be retried on the next tick.
/// </summary>
public sealed class ReservationExpirationProcessor(EventosVivosDbContext context, IClock clock)
{
    private const int BatchSize = 100;

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var now = clock.UtcNow.UtcDateTime;

        var expired = await context.Reservations
            .Where(r => r.Status == ReservationStatus.PendingPayment && r.ExpiresAtUtc <= now)
            .OrderBy(r => r.ExpiresAtUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            return;
        }

        foreach (var reservation in expired)
        {
            var @event = await context.Events
                .FirstOrDefaultAsync(e => e.Id == reservation.EventId, cancellationToken);
            if (@event is null)
            {
                continue;
            }

            reservation.Expire();
            @event.ReleaseTickets(reservation.Quantity);

            var payload = JsonSerializer.Serialize(
                new TicketsReleasedIntegrationEvent(@event.Id, @event.AvailableTickets));
            context.OutboxMessages.Add(
                OutboxMessage.Create(IntegrationEventTypes.TicketsReleased, payload, now));
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
