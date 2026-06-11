using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventosVivos.Infrastructure.Messaging;

/// <summary>
/// Consumes integration events from RabbitMQ and forwards their payloads to the in-memory
/// broadcaster, which pushes them to the connected SSE clients. Each instance binds its own
/// ephemeral queue to the fanout exchange, so every app instance notifies its own clients.
/// </summary>
internal sealed class RabbitMqEventStreamConsumer(
    IConfiguration configuration,
    EventStreamBroadcaster broadcaster,
    ILogger<RabbitMqEventStreamConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RABBITMQ_HOST"] ?? "localhost",
            Port = int.TryParse(configuration["RABBITMQ_PORT"], out var port) ? port : 5672,
            UserName = configuration["RABBITMQ_DEFAULT_USER"] ?? "guest",
            Password = configuration["RABBITMQ_DEFAULT_PASS"] ?? "guest",
        };

        var connection = await ConnectWithRetryAsync(factory, stoppingToken);
        if (connection is null)
        {
            return;
        }

        await using (connection)
        {
            var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await using (channel)
            {
                await channel.ExchangeDeclareAsync(
                    RabbitMqEventBus.ExchangeName, ExchangeType.Fanout, durable: true,
                    cancellationToken: stoppingToken);
                var queue = await channel.QueueDeclareAsync(cancellationToken: stoppingToken);
                await channel.QueueBindAsync(
                    queue.QueueName, RabbitMqEventBus.ExchangeName, routingKey: string.Empty,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, args) =>
                {
                    try
                    {
                        var envelope = JsonSerializer.Deserialize<BusEnvelope>(args.Body.Span);
                        if (envelope is not null)
                        {
                            broadcaster.Publish(envelope.Payload);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to handle an integration event.");
                    }

                    await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
                };

                await channel.BasicConsumeAsync(
                    queue.QueueName, autoAck: false, consumer, cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
    }

    private async Task<IConnection?> ConnectWithRetryAsync(
        ConnectionFactory factory,
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                return await factory.CreateConnectionAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "RabbitMQ not reachable yet; retrying in 5s.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        return null;
    }
}
