using Longstone.Domain.Audit;
using Longstone.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace Longstone.Infrastructure.Persistence;

public class LongstoneDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public LongstoneDbContext(DbContextOptions<LongstoneDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LongstoneDbContext).Assembly);
    }
}
