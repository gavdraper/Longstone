using System.Security.Claims;
using Longstone.Domain.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace Longstone.Web.Auth;

public sealed class CurrentUserPermissionService(
    AuthenticationStateProvider authStateProvider,
    IPermissionService permissionService)
{
    public async Task<bool> HasPermissionAsync(Permission permission)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return false;
        return await permissionService.HasPermissionAsync(userId.Value, permission);
    }

    private async Task<Guid?> GetCurrentUserIdAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var claim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (claim is not null && Guid.TryParse(claim, out var userId))
        {
            return userId;
        }

        return null;
    }
}
