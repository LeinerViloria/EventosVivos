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

    public DateTime? CancelledAtUtc { get; private set; }

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

    /// <summary>
    /// Cancels the reservation (RF-05). A pending reservation, or a confirmed one cancelled at least
    /// 48h before the event, becomes <see cref="ReservationStatus.Cancelled"/> and its tickets are
    /// released by the caller. A confirmed reservation cancelled within 48h of the event becomes
    /// <see cref="ReservationStatus.Lost"/> (RN07): the tickets are forfeited, not released.
    /// Terminal states (cancelled, lost, expired) are rejected.
    /// </summary>
    /// <remarks>
    /// RF-05 is internally contradictory: it states that a confirmed reservation transitions to
    /// cancelled, yet also that an "already paid/confirmed" reservation must be rejected. We follow
    /// the reading that keeps the requirement most consistent — a confirmed reservation IS cancellable
    /// (honoring the explicit confirmed→cancelled transition and keeping RN07 meaningful) — and read
    /// the rejection clause as a guard against re-cancelling terminal reservations.
    /// </remarks>
    public Result Cancel(DateTime eventStartUtc, DateTimeOffset now)
    {
        switch (Status)
        {
            case ReservationStatus.PendingPayment:
                Status = ReservationStatus.Cancelled;
                break;
            case ReservationStatus.Confirmed:
                Status = eventStartUtc - now.UtcDateTime < TimeSpan.FromHours(48)
                    ? ReservationStatus.Lost
                    : ReservationStatus.Cancelled;
                break;
            default:
                return Result.Failure(ReservationErrors.NotCancellable);
        }

        CancelledAtUtc = now.UtcDateTime;
        return Result.Success();
    }

    /// <summary>True when cancellation released the tickets back for sale (cancelled, not lost).</summary>
    public bool ReleasedOnCancel => Status == ReservationStatus.Cancelled;

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
