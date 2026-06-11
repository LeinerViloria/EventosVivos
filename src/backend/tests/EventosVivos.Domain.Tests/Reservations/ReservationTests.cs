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
}
