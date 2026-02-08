using Longstone.Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Longstone.Infrastructure.Persistence.Configurations;

public class FundConfiguration : IEntityTypeConfiguration<Fund>
{
    public void Configure(EntityTypeBuilder<Fund> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(f => f.Name);

        builder.Property(f => f.Lei)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(f => f.Lei)
            .IsUnique();

        builder.Property(f => f.Isin)
            .IsRequired()
            .HasMaxLength(12);

        builder.HasIndex(f => f.Isin)
            .IsUnique();

        builder.Property(f => f.FundType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(f => f.BaseCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(f => f.BenchmarkIndex)
            .HasMaxLength(100);

        builder.Property(f => f.InceptionDate)
            .IsRequired();

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(f => f.AssignedManagers)
            .WithOne()
            .HasForeignKey(fm => fm.FundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.AssignedManagers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .IsRequired();
    }
}
