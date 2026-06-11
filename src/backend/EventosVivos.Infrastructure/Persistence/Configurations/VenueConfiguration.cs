using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

/// <summary>Identifiers of the seeded reference venues (see RN and datos de referencia).</summary>
public static class VenueIds
{
    public static readonly Guid AuditorioCentral = new("0199e000-0000-7000-8000-000000000001");
    public static readonly Guid SalaNorte = new("0199e000-0000-7000-8000-000000000002");
    public static readonly Guid ArenaSur = new("0199e000-0000-7000-8000-000000000003");
}

internal sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();
        builder.Property(v => v.City).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Capacity).IsRequired();

        // RN02: el venue gobierna la agenda de sus eventos; su versión (columna de sistema
        // xmin) protege la creación concurrente frente a solapamientos de horario.
        builder.Property<uint>("Version")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasData(
            new Venue(VenueIds.AuditorioCentral, "Auditorio Central", 200, "Bogotá"),
            new Venue(VenueIds.SalaNorte, "Sala Norte", 50, "Bogotá"),
            new Venue(VenueIds.ArenaSur, "Arena Sur", 500, "Medellín"));
    }
}
