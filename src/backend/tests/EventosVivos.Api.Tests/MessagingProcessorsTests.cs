using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Infrastructure.Messaging;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Api.Tests;

public sealed class MessagingProcessorsTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    [Fact]
    public async Task Expiration_processor_expires_releases_and_writes_an_outbox_message()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

        var venue = await db.Venues.FirstAsync();
        var start = new DateTimeOffset(2026, 12, 20, 18, 0, 0, TimeSpan.Zero);
        var @event = Event.Create(
            "Expiring Event", "An event whose hold expires.", venue, 50,
            start, start.AddHours(2), 40m, EventType.Conference).Value;
        @event.Reserve(3, DateTimeOffset.UtcNow);
        db.Events.Add(@event);

        var reservation = Reservation.CreatePending(
            @event.Id, "Ana", "ana@example.com", 3, DateTimeOffset.UtcNow.AddMinutes(-30), TimeSpan.Zero);
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        await scope.ServiceProvider
            .GetRequiredService<ReservationExpirationProcessor>()
            .ProcessAsync(CancellationToken.None);

        Assert.Equal(ReservationStatus.Expired, reservation.Status);
        Assert.Equal(0, @event.ReservedTickets);
        Assert.True(await db.OutboxMessages.AnyAsync(m =>
            m.Type == IntegrationEventTypes.TicketsReleased && m.ProcessedOnUtc == null));
    }

    [Fact]
    public async Task Completion_processor_marks_overdue_active_events_completed()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

        var venue = await db.Venues.FirstAsync();
        // A weekday event that already ended (RN03 does not apply; Create allows past dates).
        var start = new DateTimeOffset(2026, 1, 7, 18, 0, 0, TimeSpan.Zero);
        var @event = Event.Create(
            "Past Event", "An event that has already finished.", venue, 50,
            start, start.AddHours(2), 40m, EventType.Conference).Value;
        db.Events.Add(@event);
        await db.SaveChangesAsync();

        await scope.ServiceProvider
            .GetRequiredService<EventCompletionProcessor>()
            .ProcessAsync(CancellationToken.None);

        await db.Entry(@event).ReloadAsync();
        Assert.Equal(EventStatus.Completed, @event.Status);
    }

    [Fact]
    public async Task Outbox_processor_publishes_pending_messages_and_marks_them_processed()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();

        var message = OutboxMessage.Create(
            IntegrationEventTypes.TicketsReleased, "{\"eventId\":\"x\",\"availableTickets\":10}", DateTime.UtcNow);
        db.OutboxMessages.Add(message);
        await db.SaveChangesAsync();

        await scope.ServiceProvider
            .GetRequiredService<OutboxProcessor>()
            .ProcessAsync(CancellationToken.None);

        var processed = await db.OutboxMessages.FindAsync(message.Id);
        Assert.NotNull(processed!.ProcessedOnUtc);

        var bus = (RecordingEventBus)factory.Services.GetRequiredService<IEventBus>();
        Assert.Contains(bus.Published, published => published.Type == IntegrationEventTypes.TicketsReleased);
    }
}
