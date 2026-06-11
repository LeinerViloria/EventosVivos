using System.Net;
using EventosVivos.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Api.Tests;

public sealed class EventStreamEndpointTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    [Fact]
    public async Task Requires_authentication()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/v1/events/stream", HttpCompletionOption.ResponseHeadersRead);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Streams_broadcast_events_using_a_query_string_token()
    {
        var adminClient = await factory.CreateAdminClientAsync();
        var token = adminClient.DefaultRequestHeaders.Authorization!.Parameter!;

        var client = factory.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        var response = await client.GetAsync(
            $"/api/v1/events/stream?access_token={token}",
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        factory.Services
            .GetRequiredService<EventStreamBroadcaster>()
            .Publish("{\"eventId\":\"abc\",\"availableTickets\":7}");

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        string? dataLine = null;
        while (!cts.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cts.Token);
            if (line is null)
            {
                break;
            }

            if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                dataLine = line;
                break;
            }
        }

        Assert.NotNull(dataLine);
        Assert.Contains("eventId", dataLine);
    }
}
