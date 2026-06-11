using EventosVivos.Application.Features.Venues.SearchVenues;
using Mediator;

namespace EventosVivos.Api.Endpoints.Venues;

public sealed class SearchVenuesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/venues/search", async (
            string? term,
            int? limit,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new SearchVenuesQuery(term, limit ?? 20), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Venues");
    }
}
