using System.Text.Json;
using EventosVivos.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace EventosVivos.Infrastructure.Messaging;

/// <summary>The envelope published to the bus: the integration event type plus its JSON payload.</summary>
public sealed record BusEnvelope(string Type, string Payload);

/// <summary>
/// Publishes integration events to a durable fanout exchange in RabbitMQ. The connection and
/// channel are created lazily and reused; failures bubble up so the Outbox publisher can retry.
/// </summary>
internal sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    public const string ExchangeName = "eventosvivos.events";

    private readonly ConnectionFactory _factory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqEventBus(IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration["RABBITMQ_HOST"] ?? "localhost",
            Port = int.TryParse(configuration["RABBITMQ_PORT"], out var port) ? port : 5672,
            UserName = configuration["RABBITMQ_DEFAULT_USER"] ?? "guest",
            Password = configuration["RABBITMQ_DEFAULT_PASS"] ?? "guest",
        };
    }

    public async Task PublishAsync(string type, string payload, CancellationToken cancellationToken)
    {
        var channel = await GetChannelAsync(cancellationToken);
        var body = JsonSerializer.SerializeToUtf8Bytes(new BusEnvelope(type, payload));

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body,
            cancellationToken: cancellationToken);
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _connection ??= await _factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await _channel.ExchangeDeclareAsync(
                ExchangeName, ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken);
            return _channel;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _gate.Dispose();
    }
}
