using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventosVivos.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools to create the context when generating
/// migrations. Reads the connection from environment variables (loaded from the .env), so
/// no value is hardcoded. Generating migrations does not open a connection.
/// </summary>
public sealed class EventosVivosDbContextFactory : IDesignTimeDbContextFactory<EventosVivosDbContext>
{
    public EventosVivosDbContext CreateDbContext(string[] args)
    {
        var connectionString = PostgresConnectionString.Build(Environment.GetEnvironmentVariable);

        var options = new DbContextOptionsBuilder<EventosVivosDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new EventosVivosDbContext(options);
    }
}
