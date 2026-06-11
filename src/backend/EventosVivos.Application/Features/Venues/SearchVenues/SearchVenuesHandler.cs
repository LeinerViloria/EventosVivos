using EventosVivos.Domain.Venues;
using Mediator;

namespace EventosVivos.Application.Features.Venues.SearchVenues;

public sealed class SearchVenuesHandler(IVenueRepository venues)
    : IRequestHandler<SearchVenuesQuery, IReadOnlyList<VenueSearchItem>>
{
    public async ValueTask<IReadOnlyList<VenueSearchItem>> Handle(
        SearchVenuesQuery query,
        CancellationToken cancellationToken)
    {
        var matches = await venues.SearchAsync(query.Term, query.Limit, cancellationToken);

        return matches
            .Select(venue => new VenueSearchItem(venue.Id, venue.Name, venue.Capacity, venue.City))
            .ToList();
    }
}
