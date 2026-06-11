using EventosVivos.Application.Abstractions;

namespace EventosVivos.Api.Tests;

/// <summary>A test double for <see cref="IEventBus"/> that records what was published.</summary>
public sealed class RecordingEventBus : IEventBus
{
    private readonly List<(string Type, string Payload)> _published = [];

    public IReadOnlyList<(string Type, string Payload)> Published => _published;

    public Task PublishAsync(string type, string payload, CancellationToken cancellationToken)
    {
        _published.Add((type, payload));
        return Task.CompletedTask;
    }
}
