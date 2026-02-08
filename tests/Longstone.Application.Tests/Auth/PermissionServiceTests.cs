using FluentAssertions;
using Longstone.Domain.Auth;
using Longstone.Infrastructure.Auth;
using Longstone.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Longstone.Application.Tests.Auth;

public class PermissionServiceTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LongstoneDbContext _dbContext;
    private readonly PermissionService _sut;
    private readonly TimeProvider _timeProvider = TimeProvider.System;

    public PermissionServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LongstoneDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new LongstoneDbContext(options);
        _dbContext.Database.EnsureCreated();

        _sut = new PermissionService(_dbContext, NullLogger<PermissionService>.Instance);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private User CreateUser(Role role, string username = "testuser")
    {
        var user = User.Create(username, $"{username}@test.com", "Test User", role, "hash", _timeProvider);
        _dbContext.Users.Add(user);
        return user;
    }

    private void AddRolePermission(Role role, Permission permission, PermissionScope scope)
    {
        _dbContext.RolePermissions.Add(RolePermission.Create(role, permission, scope));
    }

    private void AddUserOverride(Guid userId, Permission permission, PermissionScope scope, bool isGranted)
    {
        var adminId = Guid.NewGuid();
        _dbContext.UserPermissionOverrides.Add(
            UserPermissionOverride.Create(userId, permission, scope, isGranted, adminId, "Test override", _timeProvider));
    }

    [Fact]
    public async Task HasPermission_UserWithNoOverridesAndNoRoleDefault_ReturnsFalse()
    {
        var user = CreateUser(Role.ReadOnly, "nodefaults");
        await _dbContext.SaveChangesAsync();

        var result = await _sut.HasPermissionAsync(user.Id, Permission.ManageUsers);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_UserWithRoleDefault_ReturnsTrue()
    {
        var user = CreateUser(Role.FundManager, "fundmgr");
        AddRolePermission(Role.FundManager, Permission.ViewPortfolios, PermissionScope.Own);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.HasPermissionAsync(user.Id, Permission.ViewPortfolios);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_UserWithGrantOverrideBeyondRole_ReturnsTrue()
    {
        var user = CreateUser(Role.FundManager, "fundmgr_override");
        // FundManager has no ManageUsers by default
        AddUserOverride(user.Id, Permission.ManageUsers, PermissionScope.All, isGranted: true);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.HasPermissionAsync(user.Id, Permission.ManageUsers);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_UserWithDenyOverride_LosesRoleDefaultAccess()
    {
        var user = CreateUser(Role.FundManager, "fundmgr_denied");
        AddRolePermission(Role.FundManager, Permission.ViewPortfolios, PermissionScope.Own);
        AddUserOverride(user.Id, Permission.ViewPortfolios, PermissionScope.Own, isGranted: false);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.HasPermissionAsync(user.Id, Permission.ViewPortfolios);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_SystemAdmin_AlwaysHasAllPermissions()
    {
        var user = CreateUser(Role.SystemAdmin, "admin_all");
        // No role permissions seeded — SystemAdmin should still have all
        await _dbContext.SaveChangesAsync();

        foreach (var permission in Enum.GetValues<Permission>())
        {
            var result = await _sut.HasPermissionAsync(user.Id, permission);
            result.Should().BeTrue($"SystemAdmin should have {permission}");
        }
    }

    [Fact]
    public async Task HasPermission_SystemAdmin_IgnoresDenyOverride()
    {
        var user = CreateUser(Role.SystemAdmin, "admin_deny");
        AddUserOverride(user.Id, Permission.ViewPortfolios, PermissionScope.All, isGranted: false);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.HasPermissionAsync(user.Id, Permission.ViewPortfolios);

        result.Should().BeTrue("SystemAdmin bypasses all permission checks including deny overrides");
    }

    [Fact]
    public async Task GetPermissionScope_UserWithRoleDefault_ReturnsRoleScope()
    {
        var user = CreateUser(Role.FundManager, "scope_role");
        AddRolePermission(Role.FundManager, Permission.ViewPortfolios, PermissionScope.Own);
        await _dbContext.SaveChangesAsync();

        var scope = await _sut.GetPermissionScopeAsync(user.Id, Permission.ViewPortfolios);

        scope.Should().Be(PermissionScope.Own);
    }

    [Fact]
    public async Task GetPermissionScope_UserWithOverrideEscalation_ReturnsOverrideScope()
    {
        var user = CreateUser(Role.FundManager, "scope_override");
        AddRolePermission(Role.FundManager, Permission.ViewPortfolios, PermissionScope.Own);
        AddUserOverride(user.Id, Permission.ViewPortfolios, PermissionScope.All, isGranted: true);
        await _dbContext.SaveChangesAsync();

        var scope = await _sut.GetPermissionScopeAsync(user.Id, Permission.ViewPortfolios);

        scope.Should().Be(PermissionScope.All);
    }

    [Fact]
    public async Task GetPermissionScope_UserWithNoPermission_ReturnsNull()
    {
        var user = CreateUser(Role.ReadOnly, "noscope");
        await _dbContext.SaveChangesAsync();

        var scope = await _sut.GetPermissionScopeAsync(user.Id, Permission.ManageUsers);

        scope.Should().BeNull();
    }

    [Fact]
    public async Task GetPermissionScope_SystemAdmin_ReturnsAll()
    {
        var user = CreateUser(Role.SystemAdmin, "admin_scope");
        await _dbContext.SaveChangesAsync();

        var scope = await _sut.GetPermissionScopeAsync(user.Id, Permission.ManageUsers);

        scope.Should().Be(PermissionScope.All);
    }

    [Fact]
    public async Task GetEffectivePermissions_FundManager_ReturnsRoleDefaults()
    {
        var user = CreateUser(Role.FundManager, "effective_role");
        AddRolePermission(Role.FundManager, Permission.ViewPortfolios, PermissionScope.Own);
        AddRolePermission(Role.FundManager, Permission.CreateOrders, PermissionScope.Own);
        AddRolePermission(Role.FundManager, Permission.ViewRiskDashboards, PermissionScope.Own);
        await _dbContext.SaveChangesAsync();

        var permissions = await _sut.GetEffectivePermissionsAsync(user.Id);

        permissions.Should().HaveCount(11, "should include all permissions, granted or not");

        var viewPortfolios = permissions.Single(p => p.Permission == Permission.ViewPortfolios);
        viewPortfolios.IsGranted.Should().BeTrue();
        viewPortfolios.Scope.Should().Be(PermissionScope.Own);
        viewPortfolios.Source.Should().Be(PermissionGrantSource.RoleDefault);

        var manageUsers = permissions.Single(p => p.Permission == Permission.ManageUsers);
        manageUsers.IsGranted.Should().BeFalse();
        manageUsers.Source.Should().Be(PermissionGrantSource.Default);
    }

    [Fact]
    public async Task GetEffectivePermissions_WithOverride_ShowsOverrideSource()
    {
        var user = CreateUser(Role.FundManager, "effective_override");
        AddRolePermission(Role.FundManager, Permission.ViewPortfolios, PermissionScope.Own);
        AddUserOverride(user.Id, Permission.ViewPortfolios, PermissionScope.All, isGranted: true);
        AddUserOverride(user.Id, Permission.ManageUsers, PermissionScope.All, isGranted: true);
        await _dbContext.SaveChangesAsync();

        var permissions = await _sut.GetEffectivePermissionsAsync(user.Id);

        var viewPortfolios = permissions.Single(p => p.Permission == Permission.ViewPortfolios);
        viewPortfolios.IsGranted.Should().BeTrue();
        viewPortfolios.Scope.Should().Be(PermissionScope.All);
        viewPortfolios.Source.Should().Be(PermissionGrantSource.UserOverride);

        var manageUsers = permissions.Single(p => p.Permission == Permission.ManageUsers);
        manageUsers.IsGranted.Should().BeTrue();
        manageUsers.Scope.Should().Be(PermissionScope.All);
        manageUsers.Source.Should().Be(PermissionGrantSource.UserOverride);
    }

    [Fact]
    public async Task GetEffectivePermissions_SystemAdmin_AllGrantedWithAllScope()
    {
        var user = CreateUser(Role.SystemAdmin, "effective_admin");
        await _dbContext.SaveChangesAsync();

        var permissions = await _sut.GetEffectivePermissionsAsync(user.Id);

        permissions.Should().HaveCount(11);
        permissions.Should().AllSatisfy(p =>
        {
            p.IsGranted.Should().BeTrue();
            p.Scope.Should().Be(PermissionScope.All);
        });
    }

    [Fact]
    public async Task HasPermission_NonExistentUser_ReturnsFalse()
    {
        var result = await _sut.HasPermissionAsync(Guid.NewGuid(), Permission.ViewPortfolios);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_InactiveUser_ReturnsFalse()
    {
        var user = CreateUser(Role.FundManager, "inactive");
        AddRolePermission(Role.FundManager, Permission.ViewPortfolios, PermissionScope.Own);
        user.Deactivate(_timeProvider);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.HasPermissionAsync(user.Id, Permission.ViewPortfolios);

        result.Should().BeFalse();
    }
}
