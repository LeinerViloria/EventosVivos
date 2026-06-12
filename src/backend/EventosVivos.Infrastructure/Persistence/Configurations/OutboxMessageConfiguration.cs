using EventosVivos.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.OccurredOnUtc).HasColumnType(ColumnTypes.TimestampWithTimeZone);
        builder.Property(m => m.ProcessedOnUtc).HasColumnType(ColumnTypes.TimestampWithTimeZone);

        // The publisher polls for unprocessed messages oldest-first.
        builder.HasIndex(m => m.ProcessedOnUtc);
    }
}
