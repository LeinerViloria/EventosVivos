using EventosVivos.Domain.Reservations;

namespace EventosVivos.Domain.Tests.Reservations;

public class ReservationTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private static Reservation APendingReservation() =>
        Reservation.CreatePending(
            Guid.CreateVersion7(), "Ana", "ana@example.com", 2, Now, TimeSpan.FromMinutes(15));

    [Fact]
    public void CreatePending_starts_in_pending_payment_and_sets_the_expiry()
    {
        var reservation = APendingReservation();

        Assert.Equal(ReservationStatus.PendingPayment, reservation.Status);
        Assert.Equal(Now.UtcDateTime.AddMinutes(15), reservation.ExpiresAtUtc);
        Assert.Equal("ana@example.com", reservation.BuyerEmail);
    }

    [Fact]
    public void Expire_moves_a_pending_reservation_to_expired()
    {
        var reservation = APendingReservation();

        reservation.Expire();

        Assert.Equal(ReservationStatus.Expired, reservation.Status);
    }

    [Fact]
    public void Confirm_stamps_the_code_and_marks_the_reservation_confirmed()
    {
        var reservation = APendingReservation();

        var result = reservation.Confirm("EV-123456", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal("EV-123456", reservation.ConfirmationCode);
        Assert.Equal(Now.UtcDateTime, reservation.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_fails_when_already_confirmed()
    {
        var reservation = APendingReservation();
        reservation.Confirm("EV-111111", Now);

        var result = reservation.Confirm("EV-222222", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_ALREADY_CONFIRMED", result.Error.Code);
    }

    [Fact]
    public void Confirm_fails_when_the_reservation_is_not_pending()
    {
        var reservation = APendingReservation();
        reservation.Expire();

        var result = reservation.Confirm("EV-333333", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_NOT_PENDING", result.Error.Code);
    }

    [Fact]
    public void Cancel_releases_tickets_and_stamps_the_time_for_a_pending_reservation()
    {
        var reservation = APendingReservation();

        var result = reservation.Cancel(Now.UtcDateTime.AddDays(3), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.True(reservation.ReleasedOnCancel);
        Assert.Equal(Now.UtcDateTime, reservation.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_releases_tickets_for_a_confirmed_reservation_at_least_48h_before_the_event()
    {
        var reservation = APendingReservation();
        reservation.Confirm("EV-123456", Now);

        var result = reservation.Cancel(Now.UtcDateTime.AddHours(48), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.True(reservation.ReleasedOnCancel);
    }

    [Fact]
    public void Cancel_marks_a_confirmed_reservation_lost_within_48h_of_the_event()
    {
        var reservation = APendingReservation();
        reservation.Confirm("EV-123456", Now);

        // RN07: cancelling a confirmed reservation under 48h forfeits the tickets.
        var result = reservation.Cancel(Now.UtcDateTime.AddHours(47), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Lost, reservation.Status);
        Assert.False(reservation.ReleasedOnCancel);
        Assert.Equal(Now.UtcDateTime, reservation.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_fails_when_the_reservation_is_already_cancelled()
    {
        var reservation = APendingReservation();
        reservation.Cancel(Now.UtcDateTime.AddDays(3), Now);

        var result = reservation.Cancel(Now.UtcDateTime.AddDays(3), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_NOT_CANCELLABLE", result.Error.Code);
    }

    [Fact]
    public void Cancel_fails_when_the_reservation_is_expired()
    {
        var reservation = APendingReservation();
        reservation.Expire();

        var result = reservation.Cancel(Now.UtcDateTime.AddDays(3), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("RESERVATION_NOT_CANCELLABLE", result.Error.Code);
    }
}
