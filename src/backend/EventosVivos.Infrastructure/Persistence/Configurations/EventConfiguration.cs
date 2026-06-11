using EventosVivos.Domain.Events;
using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired();
        builder.Property(e => e.MaxCapacity).IsRequired();
        builder.Property(e => e.ReservedTickets).IsRequired();
        builder.Property(e => e.Price).HasColumnType("numeric(10,2)");

        // El backend opera y persiste en UTC.
        builder.Property(e => e.StartUtc).HasColumnType("timestamp with time zone");
        builder.Property(e => e.EndUtc).HasColumnType("timestamp with time zone");

        // Opciones cerradas como valor numérico (enum : byte).
        builder.Property(e => e.Type).HasConversion<byte>();
        builder.Property(e => e.Status).HasConversion<byte>();

        // Concurrencia optimista con la columna de sistema xmin de Postgres. Protege el
        // contador de entradas (ReservedTickets) ante reservas concurrentes.
        builder.Property<uint>("Version")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasOne<Venue>()
            .WithMany()
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soporta la verificación de solape por venue (RN02) y el filtro por venue.
        builder.HasIndex(e => new { e.VenueId, e.StartUtc });
    }
}
