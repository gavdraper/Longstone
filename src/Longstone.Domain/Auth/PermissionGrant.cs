namespace Longstone.Domain.Auth;

public sealed record PermissionGrant
{
    public Permission Permission { get; }
    public PermissionScope? Scope { get; }
    public bool IsGranted { get; }
    public PermissionGrantSource Source { get; }

    private PermissionGrant(Permission permission, PermissionScope? scope, bool isGranted, PermissionGrantSource source)
    {
        Permission = permission;
        Scope = scope;
        IsGranted = isGranted;
        Source = source;
    }

    public static PermissionGrant FromRoleDefault(Permission permission, PermissionScope scope)
    {
        return new PermissionGrant(permission, scope, isGranted: true, PermissionGrantSource.RoleDefault);
    }

    public static PermissionGrant FromUserOverride(Permission permission, PermissionScope? scope, bool isGranted)
    {
        return new PermissionGrant(permission, isGranted ? scope : null, isGranted, PermissionGrantSource.UserOverride);
    }

    public static PermissionGrant Denied(Permission permission)
    {
        return new PermissionGrant(permission, scope: null, isGranted: false, PermissionGrantSource.Default);
    }

    public static PermissionGrant Resolve(Permission permission, PermissionGrant? roleDefault, PermissionGrant? userOverride)
    {
        if (roleDefault is not null && roleDefault.Permission != permission)
            throw new ArgumentException($"Role default grant is for {roleDefault.Permission}, expected {permission}.", nameof(roleDefault));
        if (userOverride is not null && userOverride.Permission != permission)
            throw new ArgumentException($"User override grant is for {userOverride.Permission}, expected {permission}.", nameof(userOverride));

        if (userOverride is not null)
            return userOverride;

        if (roleDefault is not null)
            return roleDefault;

        return Denied(permission);
    }
}
