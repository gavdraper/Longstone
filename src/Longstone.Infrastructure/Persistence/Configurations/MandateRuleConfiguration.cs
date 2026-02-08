using Longstone.Domain.Compliance;
using Longstone.Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Longstone.Infrastructure.Persistence.Configurations;

public class MandateRuleConfiguration : IEntityTypeConfiguration<MandateRule>
{
    public void Configure(EntityTypeBuilder<MandateRule> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne<Fund>()
            .WithMany()
            .HasForeignKey(r => r.FundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.FundId);

        builder.HasIndex(r => new { r.FundId, r.IsActive });

        builder.Property(r => r.RuleType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Parameters)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(r => r.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.Property(r => r.EffectiveFrom)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();
    }
}
