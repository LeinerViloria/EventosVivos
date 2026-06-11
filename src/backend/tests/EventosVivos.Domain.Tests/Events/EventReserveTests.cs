using EventosVivos.Domain.Events;
using EventosVivos.Domain.Venues;

namespace EventosVivos.Domain.Tests.Events;

public class EventReserveTests
{
    // Monday 2026-06-15, 18:00 UTC — a weekday so Event.Create's RN03 does not apply.
    private static readonly DateTimeOffset Start = new(2026, 6, 15, 18, 0, 0, TimeSpan.Zero);

    private static Event AnEvent(decimal price = 50m, int maxCapacity = 100)
    {
        var venue = new Venue(Guid.CreateVersion7(), "Auditorio Central", 500, "Bogotá");
        return Event.Create(
            "Tech Talk", "A talk about technology.", venue, maxCapacity,
            Start, Start.AddHours(2), price, EventType.Conference).Value;
    }

    [Fact]
    public void Reserve_holds_the_tickets_on_success()
    {
        var @event = AnEvent();

        var result = @event.Reserve(3, Start.AddHours(-48));

        Assert.True(result.IsSuccess);
        Assert.Equal(3, @event.ReservedTickets);
        Assert.Equal(97, @event.AvailableTickets);
    }

    [Fact]
    public void Reserve_fails_when_the_event_starts_within_an_hour()
    {
        var result = AnEvent().Reserve(1, Start.AddMinutes(-30));

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_TOO_LATE", result.Error.Code);
    }

    [Fact]
    public void Reserve_caps_at_five_within_twenty_four_hours()
    {
        var @event = AnEvent();

        Assert.Equal("RESERVATION_LIMIT_EXCEEDED", @event.Reserve(6, Start.AddHours(-2)).Error.Code);
        Assert.True(@event.Reserve(5, Start.AddHours(-2)).IsSuccess);
    }

    [Fact]
    public void Reserve_caps_at_ten_for_events_over_one_hundred()
    {
        var @event = AnEvent(price: 150m);

        var result = @event.Reserve(11, Start.AddHours(-48));

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_LIMIT_EXCEEDED", result.Error.Code);
    }

    [Fact]
    public void Reserve_fails_when_there_are_not_enough_tickets()
    {
        var @event = AnEvent(maxCapacity: 2);

        var result = @event.Reserve(3, Start.AddHours(-48));

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_NO_TICKETS_AVAILABLE", result.Error.Code);
        Assert.Equal(0, @event.ReservedTickets);
    }

    [Fact]
    public void ReleaseTickets_returns_held_tickets_to_availability()
    {
        var @event = AnEvent(maxCapacity: 10);
        @event.Reserve(4, Start.AddHours(-48));

        @event.ReleaseTickets(3);

        Assert.Equal(1, @event.ReservedTickets);
        Assert.Equal(9, @event.AvailableTickets);
    }
}
