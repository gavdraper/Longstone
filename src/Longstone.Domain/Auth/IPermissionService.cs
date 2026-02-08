namespace Longstone.Domain.Auth;

public record EffectivePermission(Permission Permission, PermissionScope? Scope, PermissionGrantSource Source, bool IsGranted);

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default);
    Task<PermissionScope?> GetPermissionScopeAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EffectivePermission>> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
