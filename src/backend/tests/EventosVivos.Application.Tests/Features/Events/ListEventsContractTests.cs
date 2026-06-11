using EventosVivos.Application.Features.Events.ListEvents;
using EventosVivos.Domain.Events;

namespace EventosVivos.Application.Tests.Features.Events;

public class ListEventsContractTests
{
    [Fact]
    public void Query_defaults_to_the_first_page_with_no_filters()
    {
        var query = new ListEventsQuery();

        Assert.Null(query.Type);
        Assert.Null(query.Status);
        Assert.Null(query.VenueId);
        Assert.Null(query.StartFrom);
        Assert.Null(query.StartTo);
        Assert.Null(query.Title);
        Assert.Equal(1, query.Page);
        Assert.Equal(10, query.PageSize);
    }

    [Fact]
    public void Query_keeps_the_provided_filters()
    {
        var venueId = Guid.CreateVersion7();
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(30);

        var query = new ListEventsQuery(
            EventType.Concert, EventStatus.Active, venueId, from, to, "jazz", 2, 25);

        Assert.Equal(EventType.Concert, query.Type);
        Assert.Equal(EventStatus.Active, query.Status);
        Assert.Equal(venueId, query.VenueId);
        Assert.Equal(from, query.StartFrom);
        Assert.Equal(to, query.StartTo);
        Assert.Equal("jazz", query.Title);
        Assert.Equal(2, query.Page);
        Assert.Equal(25, query.PageSize);
    }

    [Fact]
    public void ListItem_exposes_the_projected_event_and_venue_fields()
    {
        var id = Guid.CreateVersion7();
        var venueId = Guid.CreateVersion7();
        var start = new DateTime(2026, 12, 1, 18, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(2);

        var item = new EventListItem(
            id, "Jazz Night", venueId, "Teatro Colón", start, end, 100, 10, 50m,
            EventType.Concert, EventStatus.Active);

        Assert.Equal(id, item.Id);
        Assert.Equal("Jazz Night", item.Title);
        Assert.Equal(venueId, item.VenueId);
        Assert.Equal("Teatro Colón", item.VenueName);
        Assert.Equal(start, item.StartUtc);
        Assert.Equal(end, item.EndUtc);
        Assert.Equal(100, item.MaxCapacity);
        Assert.Equal(10, item.ReservedTickets);
        Assert.Equal(50m, item.Price);
        Assert.Equal(EventType.Concert, item.Type);
        Assert.Equal(EventStatus.Active, item.Status);
    }
}
