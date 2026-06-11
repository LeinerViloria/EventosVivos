namespace EventosVivos.Application.Abstractions;

/// <summary>
/// Publishes an integration event to the message bus (RabbitMQ). Driven by the Outbox publisher,
/// so a message is only sent after its business transaction committed.
/// </summary>
public interface IEventBus
{
    Task PublishAsync(string type, string payload, CancellationToken cancellationToken);
}
