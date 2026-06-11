using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Reservations;

public static class ReservationErrors
{
    /// <summary>The event is cancelled or completed, so it no longer accepts reservations.</summary>
    public static readonly Error EventNotActive = new("RESERVATION_EVENT_NOT_ACTIVE");

    /// <summary>RN04: the event starts in less than one hour.</summary>
    public static readonly Error TooLate = new("RESERVATION_TOO_LATE");

    /// <summary>RN05 / RF-03: the requested quantity exceeds the per-transaction limit.</summary>
    public static Error QuantityLimitExceeded(int limit) =>
        new("RESERVATION_LIMIT_EXCEEDED", new Dictionary<string, object?> { ["limit"] = limit });

    /// <summary>Not enough tickets are available for the requested quantity.</summary>
    public static Error NoTicketsAvailable(int available) =>
        new("RESERVATION_NO_TICKETS_AVAILABLE", new Dictionary<string, object?> { ["available"] = available });
}
