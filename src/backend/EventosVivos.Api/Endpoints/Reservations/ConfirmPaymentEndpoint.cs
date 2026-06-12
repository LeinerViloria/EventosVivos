using EventosVivos.Api.Errors;
using EventosVivos.Application.Features.Reservations.ConfirmPayment;
using EventosVivos.Application.Security;
using Mediator;

namespace EventosVivos.Api.Endpoints.Reservations;

public sealed class ConfirmPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/reservations/{id:guid}/confirm", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ConfirmPaymentCommand(id), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.ToProblemResult();
        })
        .WithTags("Reservations")
        .RequireAuthorization(Permissions.ReservationsConfirm);
    }
}
