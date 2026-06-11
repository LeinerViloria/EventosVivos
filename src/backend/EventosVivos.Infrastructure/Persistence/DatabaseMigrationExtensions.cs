using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Infrastructure.Persistence;

/// <summary>
/// Applies pending EF Core migrations at startup. The seed data is baked into the migrations
/// through <c>HasData</c>, so migrating also provisions the seeded venues.
/// </summary>
public static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
