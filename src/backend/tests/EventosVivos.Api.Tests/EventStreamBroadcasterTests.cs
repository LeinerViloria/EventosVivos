using EventosVivos.Infrastructure.Messaging;

namespace EventosVivos.Api.Tests;

public class EventStreamBroadcasterTests
{
    [Fact]
    public async Task Publishes_to_each_subscriber()
    {
        var broadcaster = new EventStreamBroadcaster();
        var (_, reader) = broadcaster.Subscribe();

        broadcaster.Publish("payload");

        Assert.Equal("payload", await reader.ReadAsync());
    }

    [Fact]
    public void Stops_delivering_after_unsubscribe()
    {
        var broadcaster = new EventStreamBroadcaster();
        var (id, reader) = broadcaster.Subscribe();

        broadcaster.Unsubscribe(id);
        broadcaster.Publish("payload");

        Assert.False(reader.TryRead(out _));
    }
}
