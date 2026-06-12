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
        builder.Property(r => r.Status).HasColumnType(ColumnTypes.SmallInt);
        builder.Property(r => r.CreatedAtUtc).HasColumnType(ColumnTypes.TimestampWithTimeZone);
        builder.Property(r => r.ExpiresAtUtc).HasColumnType(ColumnTypes.TimestampWithTimeZone);
        builder.Property(r => r.ConfirmedAtUtc).HasColumnType(ColumnTypes.TimestampWithTimeZone);
        builder.Property(r => r.CancelledAtUtc).HasColumnType(ColumnTypes.TimestampWithTimeZone);
        builder.Property(r => r.ConfirmationCode).HasMaxLength(20);

        // Unique among confirmed reservations; Postgres allows multiple NULLs (pending ones).
        builder.HasIndex(r => r.ConfirmationCode).IsUnique();

        builder
            .HasOne<Event>()
            .WithMany()
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
