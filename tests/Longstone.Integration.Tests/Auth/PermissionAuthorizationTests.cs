using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Longstone.Domain.Auth;
using Longstone.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Longstone.Integration.Tests.Auth;

public class PermissionAuthorizationTests : IClassFixture<LongstoneWebApplicationFactory>
{
    private readonly LongstoneWebApplicationFactory _factory;

    public PermissionAuthorizationTests(LongstoneWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FundManager_HasViewPortfoliosPermission()
    {
        using var scope = _factory.Services.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "fundmgr");

        var hasPermission = await permissionService.HasPermissionAsync(user.Id, Permission.ViewPortfolios);

        hasPermission.Should().BeTrue();
    }

    [Fact]
    public async Task FundManager_DoesNotHaveManageUsersPermission()
    {
        using var scope = _factory.Services.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "fundmgr");

        var hasPermission = await permissionService.HasPermissionAsync(user.Id, Permission.ManageUsers);

        hasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task FundManager_DoesNotHaveViewAuditLogsPermission()
    {
        using var scope = _factory.Services.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "fundmgr");

        var hasPermission = await permissionService.HasPermissionAsync(user.Id, Permission.ViewAuditLogs);

        hasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task FundManager_ViewPortfoliosScope_IsOwn()
    {
        using var scope = _factory.Services.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "fundmgr");

        var permissionScope = await permissionService.GetPermissionScopeAsync(user.Id, Permission.ViewPortfolios);

        permissionScope.Should().Be(PermissionScope.Own);
    }

    [Fact]
    public async Task FundManager_WithGrantOverride_GetsAllScope()
    {
        using var scope = _factory.Services.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "fundmgr");
        var admin = await dbContext.Users.FirstAsync(u => u.Username == "admin");

        var existingOverride = await dbContext.UserPermissionOverrides
            .FirstOrDefaultAsync(o => o.UserId == user.Id && o.Permission == Permission.ViewPortfolios);
        if (existingOverride is not null)
        {
            dbContext.UserPermissionOverrides.Remove(existingOverride);
            await dbContext.SaveChangesAsync();
        }

        var overrideEntity = UserPermissionOverride.Create(
            user.Id, Permission.ViewPortfolios, PermissionScope.All,
            isGranted: true, admin.Id, "Test: escalate to All scope", TimeProvider.System);
        dbContext.UserPermissionOverrides.Add(overrideEntity);
        await dbContext.SaveChangesAsync();

        var permissionScope = await permissionService.GetPermissionScopeAsync(user.Id, Permission.ViewPortfolios);

        permissionScope.Should().Be(PermissionScope.All);

        // Cleanup
        dbContext.UserPermissionOverrides.Remove(overrideEntity);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task FundManager_WithDenyOverride_LosesRoleDefaultAccess()
    {
        using var scope = _factory.Services.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "fundmgr");
        var admin = await dbContext.Users.FirstAsync(u => u.Username == "admin");

        var existingOverride = await dbContext.UserPermissionOverrides
            .FirstOrDefaultAsync(o => o.UserId == user.Id && o.Permission == Permission.ViewPortfolios);
        if (existingOverride is not null)
        {
            dbContext.UserPermissionOverrides.Remove(existingOverride);
            await dbContext.SaveChangesAsync();
        }

        var overrideEntity = UserPermissionOverride.Create(
            user.Id, Permission.ViewPortfolios, PermissionScope.Own,
            isGranted: false, admin.Id, "Test: deny access", TimeProvider.System);
        dbContext.UserPermissionOverrides.Add(overrideEntity);
        await dbContext.SaveChangesAsync();

        var hasPermission = await permissionService.HasPermissionAsync(user.Id, Permission.ViewPortfolios);

        hasPermission.Should().BeFalse("deny override should block even role-default access");

        // Cleanup
        dbContext.UserPermissionOverrides.Remove(overrideEntity);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task SystemAdmin_HasAllPermissions()
    {
        using var scope = _factory.Services.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "admin");

        foreach (var permission in Enum.GetValues<Permission>())
        {
            var hasPermission = await permissionService.HasPermissionAsync(user.Id, permission);
            hasPermission.Should().BeTrue($"SystemAdmin should have {permission}");
        }
    }

    [Fact]
    public async Task AuthorizationHandler_GrantsPermissionForAuthorizedUser()
    {
        using var scope = _factory.Services.CreateScope();
        var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "admin");

        var claims = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        ], "TestAuth"));

        var result = await authorizationService.AuthorizeAsync(claims, "Permission:ManageUsers");

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizationHandler_DeniesPermissionForUnauthorizedUser()
    {
        using var scope = _factory.Services.CreateScope();
        var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "fundmgr");

        var claims = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        ], "TestAuth"));

        var result = await authorizationService.AuthorizeAsync(claims, "Permission:ManageUsers");

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task ReadOnlyUser_OnlyHasViewPortfoliosPermission()
    {
        using var scope = _factory.Services.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user = await dbContext.Users.FirstAsync(u => u.Username == "readonly");

        var effective = await permissionService.GetEffectivePermissionsAsync(user.Id);

        var granted = effective.Where(p => p.IsGranted).ToList();
        granted.Should().ContainSingle()
            .Which.Permission.Should().Be(Permission.ViewPortfolios);
    }
}
