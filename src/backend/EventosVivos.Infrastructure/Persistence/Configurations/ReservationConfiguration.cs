using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.BuyerName).HasMaxLength(120).IsRequired();
        builder.Property(r => r.BuyerEmail).HasMaxLength(256).IsRequired();
        builder.Property(r => r.Quantity);
        builder.Property(r => r.Status).HasColumnType("smallint");
        builder.Property(r => r.CreatedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(r => r.ExpiresAtUtc).HasColumnType("timestamp with time zone");

        builder
            .HasOne<Event>()
            .WithMany()
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
