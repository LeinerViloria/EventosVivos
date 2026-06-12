using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ConfirmPayment;
using EventosVivos.Domain.Reservations;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Reservations;

public class ConfirmPaymentHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly IReservationCodeGenerator _codeGenerator = Substitute.For<IReservationCodeGenerator>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly ConfirmPaymentHandler _handler;

    public ConfirmPaymentHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _handler = new ConfirmPaymentHandler(_reservations, _codeGenerator, _clock);
    }

    private static Reservation APendingReservation() =>
        Reservation.CreatePending(
            Guid.CreateVersion7(), "Ana", "ana@example.com", 2, Now, TimeSpan.FromMinutes(15));

    [Fact]
    public async Task Fails_when_the_reservation_does_not_exist()
    {
        var id = Guid.CreateVersion7();
        _reservations.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Reservation?)null);

        var result = await _handler.Handle(new ConfirmPaymentCommand(id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task Confirms_the_reservation_and_returns_the_code()
    {
        var reservation = APendingReservation();
        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _codeGenerator.Generate().Returns("EV-654321");
        _reservations.CodeExistsAsync("EV-654321", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(
            new ConfirmPaymentCommand(reservation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("EV-654321", result.Value.ConfirmationCode);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal("EV-654321", reservation.ConfirmationCode);
    }

    [Fact]
    public async Task Fails_when_the_reservation_is_already_confirmed()
    {
        var reservation = APendingReservation();
        reservation.Confirm("EV-000001", Now);
        _reservations.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);
        _codeGenerator.Generate().Returns("EV-654321");
        _reservations.CodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(
            new ConfirmPaymentCommand(reservation.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_ALREADY_CONFIRMED", result.Error.Code);
    }
}
