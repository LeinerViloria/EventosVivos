using System.Net;
using System.Net.Http.Json;
using EventosVivos.Infrastructure.Persistence.Configurations;

namespace EventosVivos.Api.Tests;

public sealed class AuthorizationTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    private static object AnEventRequest() =>
        new
        {
            title = "Authorized Event",
            description = "An event used to check authorization.",
            venueId = VenueIds.ArenaSur,
            maxCapacity = 100,
            startsAt = new DateTimeOffset(2026, 11, 20, 18, 0, 0, TimeSpan.Zero),
            endsAt = new DateTimeOffset(2026, 11, 20, 20, 0, 0, TimeSpan.Zero),
            price = 50m,
            type = 1,
        };

    [Fact]
    public async Task Protected_endpoint_returns_401_without_a_token()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/events");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_user_without_permission_gets_403_when_creating_an_event()
    {
        var client = await factory.CreateUserClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/events", AnEventRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_user_can_list_events()
    {
        var client = await factory.CreateUserClientAsync();

        var response = await client.GetAsync("/api/v1/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_revokes_the_session()
    {
        var client = await factory.CreateAdminClientAsync();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/events")).StatusCode);

        var logout = await client.PostAsync("/api/v1/auth/logout", content: null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        // The identity token now points to a removed session, so it is rejected.
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/events")).StatusCode);
    }
}
