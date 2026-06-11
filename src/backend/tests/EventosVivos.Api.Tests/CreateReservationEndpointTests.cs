using System.Net;
using System.Net.Http.Json;
using EventosVivos.Infrastructure.Persistence.Configurations;

namespace EventosVivos.Api.Tests;

public sealed class CreateReservationEndpointTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    private sealed record CreatedEvent(Guid Id);

    private sealed record PagedResponse(List<EventRow> Items);

    private sealed record EventRow(Guid Id, int ReservedTickets);

    private static async Task<Guid> CreateEvent(HttpClient client, string title, int day, int maxCapacity)
    {
        var start = new DateTimeOffset(2026, 12, day, 18, 0, 0, TimeSpan.Zero);
        var request = new
        {
            title,
            description = "A reservation integration test event.",
            venueId = VenueIds.AuditorioCentral,
            maxCapacity,
            startsAt = start,
            endsAt = start.AddHours(2),
            price = 50m,
            type = 1,
        };

        var response = await client.PostAsJsonAsync("/api/v1/events", request);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatedEvent>();
        return created!.Id;
    }

    private static object AReservation(Guid eventId, int quantity) =>
        new { eventId, buyerName = "Ana Compradora", buyerEmail = "ana@example.com", quantity };

    [Fact]
    public async Task Reserves_tickets_and_returns_201()
    {
        var admin = await factory.CreateAdminClientAsync();
        var user = await factory.CreateUserClientAsync();
        var eventId = await CreateEvent(admin, "Reservable Concert", day: 1, maxCapacity: 100);

        var response = await user.PostAsJsonAsync("/api/v1/reservations", AReservation(eventId, 2));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Returns_409_when_there_are_not_enough_tickets()
    {
        var admin = await factory.CreateAdminClientAsync();
        var eventId = await CreateEvent(admin, "Tiny Event", day: 2, maxCapacity: 2);

        var response = await admin.PostAsJsonAsync("/api/v1/reservations", AReservation(eventId, 5));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/reservations", AReservation(Guid.NewGuid(), 1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Returns_404_when_the_event_does_not_exist()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync(
            "/api/v1/reservations", AReservation(Guid.NewGuid(), 1));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Does_not_oversell_under_concurrency()
    {
        var admin = await factory.CreateAdminClientAsync();
        var eventId = await CreateEvent(admin, "Concurrent Event", day: 3, maxCapacity: 3);

        var attempts = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ =>
            admin.PostAsJsonAsync("/api/v1/reservations", AReservation(eventId, 1))));

        var created = attempts.Count(response => response.StatusCode == HttpStatusCode.Created);
        Assert.True(created <= 3);

        var list = await admin.GetFromJsonAsync<PagedResponse>(
            "/api/v1/events?title=Concurrent%20Event");
        var reservedTickets = list!.Items[0].ReservedTickets;

        // The held counter matches exactly the successful reservations and never exceeds capacity.
        Assert.Equal(created, reservedTickets);
        Assert.True(reservedTickets <= 3);
    }
}
