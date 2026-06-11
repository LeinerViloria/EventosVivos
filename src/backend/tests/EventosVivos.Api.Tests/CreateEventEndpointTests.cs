using System.Net;
using System.Net.Http.Json;
using EventosVivos.Infrastructure.Persistence.Configurations;

namespace EventosVivos.Api.Tests;

public sealed class CreateEventEndpointTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    [Fact]
    public async Task Post_creates_event_and_returns_201()
    {
        var client = await factory.CreateAdminClientAsync();
        var request = new
        {
            title = "Tech Conference",
            description = "A full day of technology talks.",
            venueId = VenueIds.AuditorioCentral,
            maxCapacity = 150,
            startsAt = new DateTimeOffset(2026, 12, 1, 18, 0, 0, TimeSpan.Zero),
            endsAt = new DateTimeOffset(2026, 12, 1, 20, 0, 0, TimeSpan.Zero),
            price = 50m,
            type = 1,
        };

        var response = await client.PostAsJsonAsync("/api/v1/events", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_returns_422_when_input_is_invalid()
    {
        var client = await factory.CreateAdminClientAsync();
        var request = new
        {
            title = "no",
            description = "short",
            venueId = VenueIds.AuditorioCentral,
            maxCapacity = 150,
            startsAt = new DateTimeOffset(2026, 12, 1, 18, 0, 0, TimeSpan.Zero),
            endsAt = new DateTimeOffset(2026, 12, 1, 20, 0, 0, TimeSpan.Zero),
            price = 50m,
            type = 1,
        };

        var response = await client.PostAsJsonAsync("/api/v1/events", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Post_returns_409_when_capacity_exceeds_venue()
    {
        var client = await factory.CreateAdminClientAsync();
        var request = new
        {
            title = "Oversized Event",
            description = "An event that exceeds the venue capacity.",
            venueId = VenueIds.SalaNorte,
            maxCapacity = 100,
            startsAt = new DateTimeOffset(2026, 12, 2, 18, 0, 0, TimeSpan.Zero),
            endsAt = new DateTimeOffset(2026, 12, 2, 20, 0, 0, TimeSpan.Zero),
            price = 50m,
            type = 1,
        };

        var response = await client.PostAsJsonAsync("/api/v1/events", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
