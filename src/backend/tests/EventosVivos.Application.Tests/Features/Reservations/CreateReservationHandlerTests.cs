using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.CreateReservation;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Venues;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Reservations;

public class CreateReservationHandlerTests
{
    private static readonly DateTimeOffset Start = new(2026, 6, 15, 18, 0, 0, TimeSpan.Zero);

    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CreateReservationHandler _handler;

    public CreateReservationHandlerTests()
    {
        _handler = new CreateReservationHandler(
            _events, _reservations, new ReservationOptions { TtlMinutes = 15 }, _clock);
    }

    private static Event AnEvent(int maxCapacity = 100)
    {
        var venue = new Venue(Guid.CreateVersion7(), "Auditorio Central", 500, "Bogotá");
        return Event.Create(
            "Tech Talk", "A talk about technology.", venue, maxCapacity,
            Start, Start.AddHours(2), 50m, EventType.Conference).Value;
    }

    private static CreateReservationCommand ACommand(Guid eventId, int quantity = 2) =>
        new(eventId, "Ana Compradora", "ana@example.com", quantity);

    [Fact]
    public async Task Fails_when_the_event_does_not_exist()
    {
        var eventId = Guid.CreateVersion7();
        _events.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns((Event?)null);

        var result = await _handler.Handle(ACommand(eventId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("EVENT_NOT_FOUND", result.Error.Code);
        _reservations.DidNotReceive().Add(Arg.Any<Reservation>());
    }

    [Fact]
    public async Task Propagates_the_domain_failure_when_the_event_cannot_be_reserved()
    {
        var @event = AnEvent();
        _events.GetByIdAsync(@event.Id, Arg.Any<CancellationToken>()).Returns(@event);
        _clock.UtcNow.Returns(Start.AddMinutes(-30)); // RN04: within an hour of the start.

        var result = await _handler.Handle(ACommand(@event.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_TOO_LATE", result.Error.Code);
        _reservations.DidNotReceive().Add(Arg.Any<Reservation>());
    }

    [Fact]
    public async Task Creates_a_pending_reservation_and_holds_the_tickets_on_success()
    {
        var @event = AnEvent();
        _events.GetByIdAsync(@event.Id, Arg.Any<CancellationToken>()).Returns(@event);
        _clock.UtcNow.Returns(Start.AddHours(-48));

        var result = await _handler.Handle(ACommand(@event.Id, quantity: 4), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(4, @event.ReservedTickets);
        _reservations.Received(1).Add(Arg.Is<Reservation>(r =>
            r.EventId == @event.Id
            && r.Quantity == 4
            && r.Status == ReservationStatus.PendingPayment
            && r.BuyerEmail == "ana@example.com"));
    }
}
