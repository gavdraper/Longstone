namespace Longstone.Domain.Auth;

public class RolePermission
{
    public Guid Id { get; private set; }
    public Role Role { get; private set; }
    public Permission Permission { get; private set; }
    public PermissionScope Scope { get; private set; }

    private RolePermission() { }

    public static RolePermission Create(Role role, Permission permission, PermissionScope scope)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        if (!Enum.IsDefined(permission))
            throw new ArgumentOutOfRangeException(nameof(permission));
        if (!Enum.IsDefined(scope))
            throw new ArgumentOutOfRangeException(nameof(scope));

        return new RolePermission
        {
            Id = Guid.NewGuid(),
            Role = role,
            Permission = permission,
            Scope = scope
        };
    }
}
