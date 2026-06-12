using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Reservations;

/// <summary>
/// A ticket reservation for an event. It is born in <see cref="ReservationStatus.PendingPayment"/>
/// and holds tickets on the event until it is confirmed, cancelled or expires. Times are UTC.
/// </summary>
public sealed class Reservation
{
    private Reservation()
    {
        // Required by EF Core.
    }

    private Reservation(
        Guid id,
        Guid eventId,
        string buyerName,
        string buyerEmail,
        int quantity,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        Id = id;
        EventId = eventId;
        BuyerName = buyerName;
        BuyerEmail = buyerEmail;
        Quantity = quantity;
        Status = ReservationStatus.PendingPayment;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public string BuyerName { get; private set; } = null!;

    public string BuyerEmail { get; private set; } = null!;

    public int Quantity { get; private set; }

    public ReservationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public string? ConfirmationCode { get; private set; }

    public DateTime? ConfirmedAtUtc { get; private set; }

    /// <summary>Expires a pending reservation so its tickets are released. No-op otherwise.</summary>
    public void Expire()
    {
        if (Status == ReservationStatus.PendingPayment)
        {
            Status = ReservationStatus.Expired;
        }
    }

    /// <summary>
    /// Confirms a pending reservation (RF-04), stamping the unique code and the confirmation time.
    /// Rejects a reservation that is already confirmed or that is no longer pending.
    /// </summary>
    public Result Confirm(string confirmationCode, DateTimeOffset now)
    {
        if (Status == ReservationStatus.Confirmed)
        {
            return Result.Failure(ReservationErrors.AlreadyConfirmed);
        }

        if (Status != ReservationStatus.PendingPayment)
        {
            return Result.Failure(ReservationErrors.NotPending);
        }

        Status = ReservationStatus.Confirmed;
        ConfirmationCode = confirmationCode;
        ConfirmedAtUtc = now.UtcDateTime;
        return Result.Success();
    }

    public static Reservation CreatePending(
        Guid eventId,
        string buyerName,
        string buyerEmail,
        int quantity,
        DateTimeOffset now,
        TimeSpan timeToLive) =>
        new(
            Guid.CreateVersion7(),
            eventId,
            buyerName.Trim(),
            buyerEmail.Trim().ToLowerInvariant(),
            quantity,
            now.UtcDateTime,
            now.UtcDateTime + timeToLive);
}
