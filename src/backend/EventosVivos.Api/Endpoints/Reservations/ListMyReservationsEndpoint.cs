using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventosVivos.Application.Features.Reservations.ListMyReservations;
using EventosVivos.Application.Security;
using EventosVivos.Domain.Reservations;
using Mediator;

namespace EventosVivos.Api.Endpoints.Reservations;

public sealed class ListMyReservationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/reservations/mine", async (
            ClaimsPrincipal principal,
            ISender sender,
            CancellationToken cancellationToken,
            ReservationStatus? status,
            int? page,
            int? pageSize) =>
        {
            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(sub, out var userId))
            {
                return Results.Unauthorized();
            }

            var query = new ListMyReservationsQuery(userId, status, page ?? 1, pageSize ?? 10);
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Reservations")
        .RequireAuthorization(Permissions.ReservationsReadOwn);
    }
}
