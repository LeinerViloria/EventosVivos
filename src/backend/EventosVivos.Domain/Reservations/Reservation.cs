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
