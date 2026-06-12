using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Reservations;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.ListReservations;

/// <summary>Lists reservations (admin), optionally filtered by status, paginated on the server.</summary>
public sealed record ListReservationsQuery(
    ReservationStatus? Status = null,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<ReservationListItem>>;
