using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.CancelReservation;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Venues;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Reservations;

public class CancelReservationHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CancelReservationHandler _handler;

    public CancelReservationHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _handler = new CancelReservationHandler(_reservations, _events, _clock);
    }

    private static Event AnEventStartingAt(DateTimeOffset start, int reserved)
    {
        var venue = new Venue(Guid.CreateVersion7(), "Auditorio Central", 500, "Bogotá");
        var @event = Event.Create(
            "Tech Talk", "A talk about technology.", venue, 100,
            start, start.AddHours(2), 50m, EventType.Conference).Value;
        @event.Reserve(reserved, start.AddHours(-72));
        return @event;
    }

    private static Reservation APendingReservationFor(Guid eventId, int quantity = 4) =>
        Reservation.CreatePending(eventId, "Ana", "ana@example.com", quantity, Now, TimeSpan.FromMinutes(15));

    [Fact]
    public async Task Fails_when_the_reservation_does_not_exist()
    {
        var id = Guid.CreateVersion7();
        _reservations.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Reservation?)null);

        var result = await _handler.Handle(new CancelReservationCommand(id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task Fails_when_the_event_does_not_exist()
    {
        var reservation = APendingReservationFor(Guid.CreateVersion7());
        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _events.GetByIdAsync(reservation.EventId, Arg.Any<CancellationToken>()).Returns((Event?)null);

        var result = await _handler.Handle(
            new CancelReservationCommand(reservation.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("EVENT_NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task Cancels_a_pending_reservation_and_releases_its_tickets()
    {
        var @event = AnEventStartingAt(Now.AddDays(3), reserved: 4);
        var reservation = APendingReservationFor(@event.Id, quantity: 4);
        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _events.GetByIdAsync(@event.Id, Arg.Any<CancellationToken>()).Returns(@event);

        var result = await _handler.Handle(
            new CancelReservationCommand(reservation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, result.Value.Status);
        Assert.Equal(0, @event.ReservedTickets);
    }

    [Fact]
    public async Task Marks_a_confirmed_reservation_lost_within_48h_and_keeps_its_tickets()
    {
        var @event = AnEventStartingAt(Now.AddHours(24), reserved: 4);
        var reservation = APendingReservationFor(@event.Id, quantity: 4);
        reservation.Confirm("EV-123456", Now);
        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _events.GetByIdAsync(@event.Id, Arg.Any<CancellationToken>()).Returns(@event);

        var result = await _handler.Handle(
            new CancelReservationCommand(reservation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Lost, result.Value.Status);
        Assert.Equal(4, @event.ReservedTickets); // RN07: not released.
    }

    [Fact]
    public async Task Propagates_the_domain_failure_for_a_terminal_reservation()
    {
        var @event = AnEventStartingAt(Now.AddDays(3), reserved: 4);
        var reservation = APendingReservationFor(@event.Id, quantity: 4);
        reservation.Cancel(@event.StartUtc, Now); // already cancelled
        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _events.GetByIdAsync(@event.Id, Arg.Any<CancellationToken>()).Returns(@event);

        var result = await _handler.Handle(
            new CancelReservationCommand(reservation.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_NOT_CANCELLABLE", result.Error.Code);
    }
}
