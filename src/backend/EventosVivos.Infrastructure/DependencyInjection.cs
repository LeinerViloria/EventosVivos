using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Events.ListEvents;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.Venues;
using EventosVivos.Infrastructure.Messaging;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Persistence.Repositories;
using EventosVivos.Infrastructure.Security;
using EventosVivos.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EventosVivos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = PostgresConnectionString.Build(key => configuration[key]);

        services.AddDbContext<EventosVivosDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IEventListReader, EventListReader>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton(new ReservationOptions
        {
            TtlMinutes = int.TryParse(configuration["RESERVATION_TTL_MINUTES"], out var ttl) ? ttl : 15,
        });

        // Authentication building blocks.
        services.AddSingleton(JwtOptions.Build(key => configuration[key]));
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(BuildRedisOptions(configuration)));
        services.AddSingleton<ISessionStore, RedisSessionStore>();
        services.AddSingleton<IPermissionStore, RedisPermissionStore>();

        // Messaging: expiration sweep + Outbox publisher to RabbitMQ.
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddScoped<ReservationExpirationProcessor>();
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<ReservationExpirationService>();
        services.AddHostedService<OutboxPublisherService>();

        return services;
    }

    private static ConfigurationOptions BuildRedisOptions(IConfiguration configuration)
    {
        var host = configuration["REDIS_HOST"] ?? "localhost";
        var port = configuration["REDIS_PORT"] ?? "6379";

        var options = new ConfigurationOptions
        {
            EndPoints = { $"{host}:{port}" },
            Password = configuration["REDIS_PASSWORD"],
            AbortOnConnectFail = false,
        };

        return options;
    }
}
