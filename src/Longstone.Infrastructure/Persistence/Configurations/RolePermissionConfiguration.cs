using Longstone.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Longstone.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(rp => rp.Permission)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(rp => rp.Scope)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.HasIndex(rp => new { rp.Role, rp.Permission })
            .IsUnique();
    }
}
