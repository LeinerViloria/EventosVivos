using EventosVivos.Api.Errors;
using EventosVivos.Application.Features.Reservations.CreateReservation;
using EventosVivos.Application.Security;
using Mediator;

namespace EventosVivos.Api.Endpoints.Reservations;

public sealed class CreateReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/reservations", async (
            CreateReservationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateReservationCommand(
                request.EventId, request.BuyerName, request.BuyerEmail, request.Quantity);

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/reservations/{result.Value.Id}", result.Value)
                : result.Error.ToProblemResult();
        })
        .WithTags("Reservations")
        .RequireAuthorization(Permissions.ReservationsCreate);
    }
}

public sealed record CreateReservationRequest(
    Guid EventId,
    string BuyerName,
    string BuyerEmail,
    int Quantity);
