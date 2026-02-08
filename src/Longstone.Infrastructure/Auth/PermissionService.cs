using System.Diagnostics;
using Longstone.Domain.Auth;
using Longstone.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Longstone.Infrastructure.Auth;

public class PermissionService : IPermissionService
{
    private static readonly ActivitySource ActivitySource = new("Longstone");

    private readonly LongstoneDbContext _dbContext;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(LongstoneDbContext dbContext, ILogger<PermissionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("PermissionCheck");
        activity?.SetTag("permission.name", permission.ToString());
        activity?.SetTag("permission.user_id", userId.ToString());

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            _logger.LogDebug("Permission denied for user {UserId}: user not found or inactive", userId);
            return false;
        }

        if (user.Role == Role.SystemAdmin)
        {
            activity?.SetTag("permission.result", "granted_admin");
            return true;
        }

        var grant = await ResolvePermissionAsync(userId, user.Role, permission, cancellationToken);

        activity?.SetTag("permission.result", grant.IsGranted ? "granted" : "denied");
        activity?.SetTag("permission.source", grant.Source.ToString());

        return grant.IsGranted;
    }

    public async Task<PermissionScope?> GetPermissionScopeAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
            return null;

        if (user.Role == Role.SystemAdmin)
            return PermissionScope.All;

        var grant = await ResolvePermissionAsync(userId, user.Role, permission, cancellationToken);

        return grant.IsGranted ? grant.Scope : null;
    }

    public async Task<IReadOnlyList<EffectivePermission>> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return [];

        if (user.Role == Role.SystemAdmin)
        {
            return Enum.GetValues<Permission>()
                .Select(p => new EffectivePermission(p, PermissionScope.All, PermissionGrantSource.RoleDefault, true))
                .ToList();
        }

        var roleDefaults = await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.Role == user.Role)
            .ToListAsync(cancellationToken);

        var userOverrides = await _dbContext.UserPermissionOverrides
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .ToListAsync(cancellationToken);

        var roleDefaultsByPermission = roleDefaults.ToDictionary(rp => rp.Permission);
        var overridesByPermission = userOverrides.ToDictionary(o => o.Permission);

        return Enum.GetValues<Permission>()
            .Select(permission =>
            {
                var roleDefault = roleDefaultsByPermission.TryGetValue(permission, out var rp)
                    ? PermissionGrant.FromRoleDefault(permission, rp.Scope)
                    : null;

                var userOverride = overridesByPermission.TryGetValue(permission, out var uo)
                    ? PermissionGrant.FromUserOverride(permission, uo.Scope, uo.IsGranted)
                    : null;

                var grant = PermissionGrant.Resolve(permission, roleDefault, userOverride);

                return new EffectivePermission(grant.Permission, grant.Scope, grant.Source, grant.IsGranted);
            })
            .ToList();
    }

    private async Task<PermissionGrant> ResolvePermissionAsync(Guid userId, Role role, Permission permission, CancellationToken cancellationToken)
    {
        var roleDefault = await _dbContext.RolePermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(rp => rp.Role == role && rp.Permission == permission, cancellationToken);

        var userOverride = await _dbContext.UserPermissionOverrides
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Permission == permission, cancellationToken);

        var roleGrant = roleDefault is not null
            ? PermissionGrant.FromRoleDefault(permission, roleDefault.Scope)
            : null;

        var overrideGrant = userOverride is not null
            ? PermissionGrant.FromUserOverride(permission, userOverride.Scope, userOverride.IsGranted)
            : null;

        return PermissionGrant.Resolve(permission, roleGrant, overrideGrant);
    }
}
