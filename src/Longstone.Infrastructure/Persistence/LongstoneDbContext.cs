using Longstone.Domain.Audit;
using Longstone.Domain.Auth;
using Longstone.Domain.Common;
using Longstone.Domain.Compliance;
using Longstone.Domain.Funds;
using Longstone.Domain.Instruments;
using Microsoft.EntityFrameworkCore;

namespace Longstone.Infrastructure.Persistence;

public class LongstoneDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<FundManager> FundManagers => Set<FundManager>();
    public DbSet<MandateRule> MandateRules => Set<MandateRule>();

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
