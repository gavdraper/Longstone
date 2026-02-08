using Longstone.Domain.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Longstone.Infrastructure.Auth;

public class PermissionRequirement : IAuthorizationRequirement
{
    public Permission Permission { get; }

    public PermissionRequirement(Permission permission)
    {
        Permission = permission;
    }
}
