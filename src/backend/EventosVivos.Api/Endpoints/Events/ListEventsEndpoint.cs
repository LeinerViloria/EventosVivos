using EventosVivos.Application.Features.Events.ListEvents;
using EventosVivos.Domain.Events;
using Mediator;

namespace EventosVivos.Api.Endpoints.Events;

public sealed class ListEventsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/events", async (
            ISender sender,
            CancellationToken cancellationToken,
            EventType? type,
            EventStatus? status,
            Guid? venueId,
            DateTimeOffset? startFrom,
            DateTimeOffset? startTo,
            string? title,
            int? page,
            int? pageSize) =>
        {
            var query = new ListEventsQuery(
                type, status, venueId, startFrom, startTo, title, page ?? 1, pageSize ?? 10);

            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Events");
    }
}
