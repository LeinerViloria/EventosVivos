using EventosVivos.Application.Features.Events.CreateEvent;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Venues;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Events;

public class CreateEventHandlerTests
{
    // Monday 2026-06-15, 18:00 — weekday afternoon (RN03 does not apply).
    private static readonly DateTimeOffset Start = new(2026, 6, 15, 18, 0, 0, TimeSpan.Zero);

    private readonly IVenueRepository _venues = Substitute.For<IVenueRepository>();
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly CreateEventHandler _handler;

    public CreateEventHandlerTests()
    {
        _handler = new CreateEventHandler(_venues, _events);
    }

    private static CreateEventCommand ACommand(Guid venueId, int maxCapacity = 100) =>
        new("Tech Talk", "A talk about technology and more.", venueId, maxCapacity,
            Start, Start.AddHours(2), 50m, EventType.Conference);

    private static Venue AVenue(int capacity = 200) =>
        new(Guid.CreateVersion7(), "Auditorio Central", capacity, "Bogotá");

    [Fact]
    public async Task Returns_failure_when_venue_not_found()
    {
        var venueId = Guid.CreateVersion7();
        _venues.GetByIdAsync(venueId, Arg.Any<CancellationToken>()).Returns((Venue?)null);

        var result = await _handler.Handle(ACommand(venueId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VENUE_NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task Returns_failure_when_schedule_overlaps()
    {
        var venue = AVenue();
        _venues.GetByIdAsync(venue.Id, Arg.Any<CancellationToken>()).Returns(venue);
        _events
            .HasOverlappingActiveEventAsync(venue.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(ACommand(venue.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VENUE_SCHEDULE_OVERLAP", result.Error.Code);
    }

    [Fact]
    public async Task Propagates_domain_failure_when_capacity_exceeds_venue()
    {
        var venue = AVenue(capacity: 50);
        _venues.GetByIdAsync(venue.Id, Arg.Any<CancellationToken>()).Returns(venue);
        _events
            .HasOverlappingActiveEventAsync(venue.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(ACommand(venue.Id, maxCapacity: 100), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("EVENT_CAPACITY_EXCEEDS_VENUE", result.Error.Code);
    }

    [Fact]
    public async Task Creates_event_and_touches_venue_on_success()
    {
        var venue = AVenue(capacity: 200);
        _venues.GetByIdAsync(venue.Id, Arg.Any<CancellationToken>()).Returns(venue);
        _events
            .HasOverlappingActiveEventAsync(venue.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(ACommand(venue.Id, maxCapacity: 150), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        _events.Received(1).Add(Arg.Any<Event>());
        _venues.Received(1).Touch(venue);
    }
}
