using System.Net;
using System.Net.Http.Json;
using EventosVivos.Infrastructure.Persistence.Configurations;

namespace EventosVivos.Api.Tests;

public sealed class MyReservationsEndpointTests(EventsApiFactory factory)
    : IClassFixture<EventsApiFactory>
{
    private sealed record CreatedEvent(Guid Id);

    private sealed record CreatedReservation(Guid Id);

    private sealed record PagedResponse(List<ReservationRow> Items, int Total);

    private sealed record ReservationRow(Guid Id, string EventTitle, int Status);

    private static async Task<Guid> CreateEvent(HttpClient admin, string title, int day)
    {
        var start = new DateTimeOffset(2026, 12, day, 10, 0, 0, TimeSpan.Zero);
        var request = new
        {
            title,
            description = "An event for my-reservations tests.",
            venueId = VenueIds.AuditorioCentral,
            maxCapacity = 100,
            startsAt = start,
            endsAt = start.AddHours(2),
            price = 50m,
            type = 1,
        };
        var response = await admin.PostAsJsonAsync("/api/v1/events", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreatedEvent>())!.Id;
    }

    [Fact]
    public async Task Returns_only_the_authenticated_users_own_reservations()
    {
        var admin = await factory.CreateAdminClientAsync();
        var user = await factory.CreateUserClientAsync();
        var eventId = await CreateEvent(admin, "My Reservations Event", day: 20);

        // The user reserves with their own account email so the endpoint can match it.
        var request = new
        {
            eventId,
            buyerName = "Usuario Regular",
            buyerEmail = "usuario@eventosvivos.dev",
            quantity = 1,
        };
        var createResponse = await user.PostAsJsonAsync("/api/v1/reservations", request);
        createResponse.EnsureSuccessStatusCode();

        var response = await user.GetFromJsonAsync<PagedResponse>("/api/v1/reservations/mine");

        Assert.NotNull(response);
        Assert.True(response.Total >= 1);
        Assert.Contains(response.Items, r => r.EventTitle == "My Reservations Event");
    }

    [Fact]
    public async Task Does_not_return_reservations_made_by_other_users()
    {
        var admin = await factory.CreateAdminClientAsync();
        var user = await factory.CreateUserClientAsync();
        var eventId = await CreateEvent(admin, "Shared Event My Reservations", day: 21);

        // Admin reserves with a different email — not the user's.
        var adminRequest = new
        {
            eventId,
            buyerName = "Otro Comprador",
            buyerEmail = "otro@example.com",
            quantity = 1,
        };
        await admin.PostAsJsonAsync("/api/v1/reservations", adminRequest);

        var response = await user.GetFromJsonAsync<PagedResponse>("/api/v1/reservations/mine");

        Assert.NotNull(response);
        Assert.DoesNotContain(response.Items, r => r.EventTitle == "Shared Event My Reservations");
    }

    [Fact]
    public async Task Returns_401_without_a_token()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/reservations/mine");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Regular_user_can_access_their_own_reservations()
    {
        var user = await factory.CreateUserClientAsync();

        var response = await user.GetAsync("/api/v1/reservations/mine");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_also_access_their_own_reservations()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.GetAsync("/api/v1/reservations/mine");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
