using EventosVivos.Application.Features.Reservations.ListReservations;
using EventosVivos.Application.Security;
using EventosVivos.Domain.Reservations;
using Mediator;

namespace EventosVivos.Api.Endpoints.Reservations;

public sealed class ListReservationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/reservations", async (
            ISender sender,
            CancellationToken cancellationToken,
            ReservationStatus? status,
            int? page,
            int? pageSize) =>
        {
            var query = new ListReservationsQuery(status, page ?? 1, pageSize ?? 10);
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Reservations")
        .RequireAuthorization(Permissions.ReservationsRead);
    }
}
