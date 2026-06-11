using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence;

public sealed class EventosVivosDbContext(DbContextOptions<EventosVivosDbContext> options)
    : DbContext(options)
{
    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventosVivosDbContext).Assembly);
    }
}
