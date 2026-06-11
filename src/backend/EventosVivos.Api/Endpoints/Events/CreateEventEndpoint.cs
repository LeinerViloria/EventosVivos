using EventosVivos.Api.Errors;
using EventosVivos.Application.Features.Events.CreateEvent;
using EventosVivos.Application.Security;
using EventosVivos.Domain.Events;
using Mediator;

namespace EventosVivos.Api.Endpoints.Events;

public sealed class CreateEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/events", async (
            CreateEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateEventCommand(
                request.Title,
                request.Description,
                request.VenueId,
                request.MaxCapacity,
                request.StartsAt,
                request.EndsAt,
                request.Price,
                request.Type);

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/v1/events/{result.Value.Id}", result.Value)
                : result.Error.ToProblemResult();
        })
        .WithTags("Events")
        .RequireAuthorization(Permissions.EventsCreate);
    }
}

public sealed record CreateEventRequest(
    string Title,
    string Description,
    Guid VenueId,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal Price,
    EventType Type);
