using Mediator;

namespace EventosVivos.Application.Features.Venues.SearchVenues;

public sealed record SearchVenuesQuery(string? Term = null, int Limit = 20)
    : IRequest<IReadOnlyList<VenueSearchItem>>;
