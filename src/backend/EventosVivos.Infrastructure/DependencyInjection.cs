using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Events.ListEvents;
using EventosVivos.Application.Features.Reports.OccupancyReport;
using EventosVivos.Application.Features.Reservations.ListMyReservations;
using EventosVivos.Application.Features.Reservations.ListReservations;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.Venues;
using EventosVivos.Infrastructure.Messaging;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Persistence.Repositories;
using EventosVivos.Infrastructure.Reports;
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
        services.AddScoped<IReservationListReader, ReservationListReader>();
        services.AddScoped<IMyReservationListReader, MyReservationListReader>();
        services.AddScoped<IOccupancyReportReader, OccupancyReportReader>();
        services.AddSingleton<IOccupancyReportPdfGenerator, OccupancyReportPdfGenerator>();
        services.AddSingleton<IReservationCodeGenerator, ReservationCodeGenerator>();
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

        // Messaging: expiration sweep + Outbox publisher to RabbitMQ + real-time fan-out to SSE.
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddSingleton<EventStreamBroadcaster>();
        services.AddScoped<ReservationExpirationProcessor>();
        services.AddScoped<EventCompletionProcessor>();
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<ReservationExpirationService>();
        services.AddHostedService<EventCompletionService>();
        services.AddHostedService<OutboxPublisherService>();
        services.AddHostedService<RabbitMqEventStreamConsumer>();

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
