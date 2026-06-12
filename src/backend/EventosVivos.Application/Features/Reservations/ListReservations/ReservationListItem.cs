using EventosVivos.Domain.Reservations;

namespace EventosVivos.Application.Features.Reservations.ListReservations;

/// <summary>A row of the reservations listing (admin). Times are in UTC.</summary>
public sealed record ReservationListItem(
    Guid Id,
    Guid EventId,
    string EventTitle,
    string BuyerName,
    string BuyerEmail,
    int Quantity,
    ReservationStatus Status,
    string? ConfirmationCode,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc);
