using System.Net.Http.Json;
using EventosVivos.Infrastructure.Persistence.Configurations;

namespace EventosVivos.Api.Tests;

public sealed class ListEventsEndpointTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    private sealed record PagedResponse(List<EventRow> Items, int Total, int Page, int PageSize);

    private sealed record EventRow(Guid Id, string Title, string VenueName, int Type, int Status);

    // Each event uses a distinct day to avoid the RN02 schedule-overlap rule at the same venue.
    private static async Task CreateEvent(HttpClient client, string title, int day, int type = 1)
    {
        var start = new DateTimeOffset(2026, 12, day, 18, 0, 0, TimeSpan.Zero);
        var request = new
        {
            title,
            description = "A listing integration test event.",
            venueId = VenueIds.AuditorioCentral,
            maxCapacity = 100,
            startsAt = start,
            endsAt = start.AddHours(2),
            price = 50m,
            type,
        };

        var response = await client.PostAsJsonAsync("/api/v1/events", request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Lists_events_filtered_by_title_and_paginates_on_the_server()
    {
        var client = await factory.CreateAdminClientAsync();
        await CreateEvent(client, "Sonar Listing Alpha", day: 1);
        await CreateEvent(client, "Sonar Listing Beta", day: 2);

        var firstPage = await client.GetFromJsonAsync<PagedResponse>(
            "/api/v1/events?title=Sonar%20Listing&page=1&pageSize=1");

        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage!.Total);
        Assert.Single(firstPage.Items);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(1, firstPage.PageSize);

        var secondPage = await client.GetFromJsonAsync<PagedResponse>(
            "/api/v1/events?title=Sonar%20Listing&page=2&pageSize=1");

        Assert.Single(secondPage!.Items);
        Assert.NotEqual(firstPage.Items[0].Id, secondPage.Items[0].Id);
    }

    [Fact]
    public async Task Filters_by_type_and_searches_title_case_insensitively()
    {
        var client = await factory.CreateAdminClientAsync();
        await CreateEvent(client, "TypeFilter Workshop", day: 5, type: 2);

        var matching = await client.GetFromJsonAsync<PagedResponse>(
            "/api/v1/events?title=typefilter&type=2");

        Assert.Equal(1, matching!.Total);
        Assert.Equal("TypeFilter Workshop", matching.Items[0].Title);
        Assert.Equal("Auditorio Central", matching.Items[0].VenueName);

        var nonMatching = await client.GetFromJsonAsync<PagedResponse>(
            "/api/v1/events?title=typefilter&type=1");

        Assert.Equal(0, nonMatching!.Total);
    }

    [Fact]
    public async Task Filters_by_status_venue_and_start_date_range()
    {
        var client = await factory.CreateAdminClientAsync();
        await CreateEvent(client, "Filter Combo Event", day: 9);

        var matching = await client.GetFromJsonAsync<PagedResponse>(
            "/api/v1/events?title=Filter%20Combo&status=1"
                + $"&venueId={VenueIds.AuditorioCentral}"
                + "&startFrom=2026-12-01T00:00:00Z&startTo=2026-12-31T23:59:59Z");

        Assert.Equal(1, matching!.Total);
        Assert.Equal(1, matching.Items[0].Status);

        var cancelled = await client.GetFromJsonAsync<PagedResponse>(
            "/api/v1/events?title=Filter%20Combo&status=2");
        Assert.Equal(0, cancelled!.Total);

        var outOfRange = await client.GetFromJsonAsync<PagedResponse>(
            "/api/v1/events?title=Filter%20Combo&startTo=2026-01-01T00:00:00Z");
        Assert.Equal(0, outOfRange!.Total);
    }
}
