using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using EventosVivos.Infrastructure.Persistence.Configurations;

namespace EventosVivos.Api.Tests;

public sealed partial class ReservationsManagementTests(EventsApiFactory factory)
    : IClassFixture<EventsApiFactory>
{
    private sealed record CreatedEvent(Guid Id);

    private sealed record CreatedReservation(Guid Id);

    private sealed record ConfirmResponse(string ConfirmationCode);

    private sealed record CancelResponse(int Status);

    private sealed record PagedResponse(List<ReservationRow> Items, int Total);

    private sealed record ReservationRow(Guid Id, string EventTitle, int Status, string? ConfirmationCode);

    [GeneratedRegex(@"^EV-\d{6}$")]
    private static partial Regex CodePattern();

    private static async Task<Guid> CreateEvent(HttpClient client, string title, int day)
    {
        var start = new DateTimeOffset(2026, 12, day, 18, 0, 0, TimeSpan.Zero);
        var request = new
        {
            title,
            description = "An event for reservation management tests.",
            venueId = VenueIds.AuditorioCentral,
            maxCapacity = 100,
            startsAt = start,
            endsAt = start.AddHours(2),
            price = 50m,
            type = 1,
        };
        var response = await client.PostAsJsonAsync("/api/v1/events", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreatedEvent>())!.Id;
    }

    private static async Task<Guid> Reserve(HttpClient client, Guid eventId)
    {
        var request = new { eventId, buyerName = "Ana", buyerEmail = "ana@example.com", quantity = 2 };
        var response = await client.PostAsJsonAsync("/api/v1/reservations", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreatedReservation>())!.Id;
    }

    [Fact]
    public async Task Confirms_a_reservation_and_returns_a_code()
    {
        var admin = await factory.CreateAdminClientAsync();
        var reservationId = await Reserve(admin, await CreateEvent(admin, "Confirmable Event", day: 1));

        var response = await admin.PostAsync($"/api/v1/reservations/{reservationId}/confirm", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ConfirmResponse>();
        Assert.Matches(CodePattern(), body!.ConfirmationCode);
    }

    [Fact]
    public async Task Returns_409_when_already_confirmed()
    {
        var admin = await factory.CreateAdminClientAsync();
        var reservationId = await Reserve(admin, await CreateEvent(admin, "Twice Confirmed", day: 2));

        await admin.PostAsync($"/api/v1/reservations/{reservationId}/confirm", content: null);
        var second = await admin.PostAsync($"/api/v1/reservations/{reservationId}/confirm", content: null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Returns_404_when_the_reservation_does_not_exist()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsync(
            $"/api/v1/reservations/{Guid.NewGuid()}/confirm", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task A_regular_user_cannot_confirm()
    {
        var user = await factory.CreateUserClientAsync();

        var response = await user.PostAsync(
            $"/api/v1/reservations/{Guid.NewGuid()}/confirm", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_list_reservations()
    {
        var admin = await factory.CreateAdminClientAsync();
        await Reserve(admin, await CreateEvent(admin, "Listable Event", day: 3));

        var response = await admin.GetFromJsonAsync<PagedResponse>("/api/v1/reservations");

        Assert.True(response!.Total >= 1);
    }

    [Fact]
    public async Task A_regular_user_cannot_list_reservations()
    {
        var user = await factory.CreateUserClientAsync();

        var response = await user.GetAsync("/api/v1/reservations");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Cancels_a_pending_reservation()
    {
        var admin = await factory.CreateAdminClientAsync();
        var reservationId = await Reserve(admin, await CreateEvent(admin, "Cancelable Event", day: 4));

        var response = await admin.PostAsync($"/api/v1/reservations/{reservationId}/cancel", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CancelResponse>();
        Assert.Equal(3, body!.Status); // ReservationStatus.Cancelled
    }

    [Fact]
    public async Task Returns_409_when_already_cancelled()
    {
        var admin = await factory.CreateAdminClientAsync();
        var reservationId = await Reserve(admin, await CreateEvent(admin, "Twice Cancelled", day: 5));

        await admin.PostAsync($"/api/v1/reservations/{reservationId}/cancel", content: null);
        var second = await admin.PostAsync($"/api/v1/reservations/{reservationId}/cancel", content: null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task A_regular_user_can_cancel_a_reservation()
    {
        var admin = await factory.CreateAdminClientAsync();
        var user = await factory.CreateUserClientAsync();
        var reservationId = await Reserve(user, await CreateEvent(admin, "User Cancelable", day: 6));

        var response = await user.PostAsync($"/api/v1/reservations/{reservationId}/cancel", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
