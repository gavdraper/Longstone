using System.Security.Claims;
using Longstone.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Longstone.Infrastructure.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(IPermissionService permissionService, ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogDebug("Permission check failed: no valid user ID claim");
            return;
        }

        if (await _permissionService.HasPermissionAsync(userId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("Permission {Permission} denied for user {UserId}", requirement.Permission, userId);
        }
    }
}
