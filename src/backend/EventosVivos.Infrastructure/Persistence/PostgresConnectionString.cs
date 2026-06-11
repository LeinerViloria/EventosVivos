using Npgsql;

namespace EventosVivos.Infrastructure.Persistence;

/// <summary>
/// Builds the PostgreSQL connection string from the configuration values that originate in
/// the .env file (delivered as environment variables by docker-compose, by the launch.json
/// envFile when debugging, or by the shell when running the EF tools). No value is hardcoded.
/// </summary>
public static class PostgresConnectionString
{
    public static string Build(Func<string, string?> getValue)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = getValue("POSTGRES_HOST"),
            Port = int.TryParse(getValue("POSTGRES_PORT"), out var port) ? port : 5432,
            Database = getValue("POSTGRES_DB"),
            Username = getValue("POSTGRES_USER"),
            Password = getValue("POSTGRES_PASSWORD"),
        };

        return builder.ConnectionString;
    }
}
