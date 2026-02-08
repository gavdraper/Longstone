using Longstone.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Longstone.Infrastructure.Persistence.Configurations;

public class UserPermissionOverrideConfiguration : IEntityTypeConfiguration<UserPermissionOverride>
{
    public void Configure(EntityTypeBuilder<UserPermissionOverride> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.Permission)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.Scope)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(o => o.OverriddenBy)
            .IsRequired();

        builder.Property(o => o.OverriddenAt)
            .IsRequired();

        builder.Property(o => o.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.UserId, o.Permission })
            .IsUnique();
    }
}
