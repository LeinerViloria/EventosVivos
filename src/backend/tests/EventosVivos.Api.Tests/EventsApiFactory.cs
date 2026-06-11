using EventosVivos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace EventosVivos.Api.Tests;

public sealed class EventsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18.4-bookworm")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // The .env is not loaded in tests; supply the JWT settings the infrastructure requires.
        builder.UseSetting("JWT_SIGNING_KEY", "integration-tests-signing-key-0123456789abcdef");
        builder.UseSetting("JWT_ISSUER", "eventosvivos-tests");
        builder.UseSetting("JWT_AUDIENCE", "eventosvivos-tests");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<EventosVivosDbContext>>();
            services.RemoveAll<EventosVivosDbContext>();
            services.AddDbContext<EventosVivosDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(_redis.GetConnectionString()));
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
        await base.DisposeAsync();
    }
}
