using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Infrastructure.Messaging;

/// <summary>Runs the reservation-expiration sweep on a timer.</summary>
internal sealed class ReservationExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReservationExpirationService> logger) : BackgroundService
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
                    .GetRequiredService<ReservationExpirationProcessor>()
                    .ProcessAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Reservation expiration sweep failed.");
            }
        }
    }
}
