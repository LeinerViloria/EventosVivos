using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Infrastructure.Messaging;

/// <summary>Runs the RN06 event-completion sweep on a timer.</summary>
internal sealed class EventCompletionService(
    IServiceScopeFactory scopeFactory,
    ILogger<EventCompletionService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                await scope.ServiceProvider
                    .GetRequiredService<EventCompletionProcessor>()
                    .ProcessAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Event completion sweep failed.");
            }
        }
    }
}
