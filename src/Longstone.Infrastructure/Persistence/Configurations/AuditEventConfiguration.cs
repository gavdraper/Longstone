using Longstone.Domain.Audit;
using Longstone.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Longstone.Infrastructure.Persistence.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.UserRole)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.EntityId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.BeforeState)
            .HasColumnType("TEXT");

        builder.Property(e => e.AfterState)
            .HasColumnType("TEXT");

        builder.Property(e => e.Reason)
            .HasMaxLength(1000);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);

        builder.Property(e => e.SessionId)
            .HasMaxLength(200);

        builder.Property(e => e.CorrelationId)
            .HasMaxLength(200);

        builder.Property(e => e.TraceId)
            .HasMaxLength(200);

        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}
