using System.Collections.Concurrent;
using System.Threading.Channels;

namespace EventosVivos.Infrastructure.Messaging;

/// <summary>
/// Fans an integration-event payload out to every open SSE connection. Each subscriber gets its
/// own bounded channel; a slow client drops the oldest messages rather than blocking the others.
/// </summary>
public sealed class EventStreamBroadcaster
{
    private readonly ConcurrentDictionary<Guid, Channel<string>> _subscribers = new();

    public (Guid Id, ChannelReader<string> Reader) Subscribe()
    {
        var channel = Channel.CreateBounded<string>(
            new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.DropOldest });
        var id = Guid.NewGuid();
        _subscribers[id] = channel;
        return (id, channel.Reader);
    }

    public void Unsubscribe(Guid id)
    {
        if (_subscribers.TryRemove(id, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }

    public void Publish(string data)
    {
        foreach (var channel in _subscribers.Values)
        {
            channel.Writer.TryWrite(data);
        }
    }
}
