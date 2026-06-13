using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ListReservations;
using EventosVivos.Domain.Reservations;
using Mediator;

namespace EventosVivos.Application.Features.Reservations.ListMyReservations;

/// <summary>Lists reservations that belong to the currently signed-in user, paginated server-side.</summary>
public sealed record ListMyReservationsQuery(
    Guid UserId,
    ReservationStatus? Status = null,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<ReservationListItem>>;
