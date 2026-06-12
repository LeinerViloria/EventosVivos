using EventosVivos.Domain.Events;
using EventosVivos.Domain.Venues;

namespace EventosVivos.Domain.Tests.Events;

public class EventTests
{
    // Monday 2026-06-15, 18:00 — a weekday afternoon (RN03 does not apply).
    private static readonly DateTimeOffset Start = new(2026, 6, 15, 18, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = Start.AddHours(2);

    private static Venue AVenue(int capacity = 200) =>
        new(Guid.CreateVersion7(), "Auditorio Central", capacity, "Bogotá");

    [Fact]
    public void Create_succeeds_when_capacity_is_within_venue()
    {
        var result = Event.Create(
            "Tech Talk", "A talk about technology and more.", AVenue(200),
            150, Start, End, 50m, EventType.Conference);

        Assert.True(result.IsSuccess);
        Assert.Equal(EventStatus.Active, result.Value.Status);
        Assert.Equal(0, result.Value.ReservedTickets);
    }

    [Fact]
    public void Create_fails_when_capacity_exceeds_venue()
    {
        var result = Event.Create(
            "Tech Talk", "A talk about technology and more.", AVenue(100),
            150, Start, End, 50m, EventType.Conference);

        Assert.True(result.IsFailure);
        Assert.Equal("EVENT_CAPACITY_EXCEEDS_VENUE", result.Error.Code);
    }

    [Fact]
    public void Create_fails_when_end_is_not_after_start()
    {
        var result = Event.Create(
            "Tech Talk", "A talk about technology and more.", AVenue(),
            150, Start, Start, 50m, EventType.Conference);

        Assert.True(result.IsFailure);
        Assert.Equal("EVENT_END_NOT_AFTER_START", result.Error.Code);
    }

    [Fact]
    public void Create_fails_when_a_weekend_event_starts_after_22()
    {
        // Saturday 2026-06-13, 22:30.
        var saturdayNight = new DateTimeOffset(2026, 6, 13, 22, 30, 0, TimeSpan.Zero);

        var result = Event.Create(
            "Live Show", "An evening live concert downtown.", AVenue(),
            150, saturdayNight, saturdayNight.AddHours(2), 80m, EventType.Concert);

        Assert.True(result.IsFailure);
        Assert.Equal("EVENT_WEEKEND_NIGHT_START", result.Error.Code);
    }

    private static Event AnActiveEvent() =>
        Event.Create(
            "Tech Talk", "A talk about technology and more.", AVenue(),
            150, Start, End, 50m, EventType.Conference).Value;

    [Fact]
    public void Complete_marks_an_active_event_completed_once_its_end_has_passed()
    {
        var @event = AnActiveEvent();

        var changed = @event.Complete(End.AddSeconds(1));

        Assert.True(changed);
        Assert.Equal(EventStatus.Completed, @event.Status);
    }

    [Fact]
    public void Complete_is_a_no_op_while_the_event_has_not_ended()
    {
        var @event = AnActiveEvent();

        var changed = @event.Complete(End.AddMinutes(-1));

        Assert.False(changed);
        Assert.Equal(EventStatus.Active, @event.Status);
    }

    [Fact]
    public void Complete_does_not_change_a_non_active_event()
    {
        var @event = AnActiveEvent();
        @event.Complete(End.AddSeconds(1)); // now Completed

        var changed = @event.Complete(End.AddDays(1));

        Assert.False(changed);
        Assert.Equal(EventStatus.Completed, @event.Status);
    }
}
