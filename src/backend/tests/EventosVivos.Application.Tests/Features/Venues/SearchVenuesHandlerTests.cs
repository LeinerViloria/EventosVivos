using EventosVivos.Application.Features.Venues.SearchVenues;
using EventosVivos.Domain.Venues;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Venues;

public class SearchVenuesHandlerTests
{
    [Fact]
    public async Task Maps_matching_venues_to_search_items()
    {
        var venues = Substitute.For<IVenueRepository>();
        var venue = new Venue(Guid.CreateVersion7(), "Auditorio Central", 200, "Bogotá");
        venues
            .SearchAsync("aud", 10, Arg.Any<CancellationToken>())
            .Returns(new List<Venue> { venue });
        var handler = new SearchVenuesHandler(venues);

        var result = await handler.Handle(new SearchVenuesQuery("aud", 10), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(venue.Id, item.Id);
        Assert.Equal("Auditorio Central", item.Name);
        Assert.Equal(200, item.Capacity);
        Assert.Equal("Bogotá", item.City);
        await venues.Received(1).SearchAsync("aud", 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_an_empty_list_when_there_are_no_matches()
    {
        var venues = Substitute.For<IVenueRepository>();
        venues
            .SearchAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Venue>());
        var handler = new SearchVenuesHandler(venues);

        var result = await handler.Handle(new SearchVenuesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
