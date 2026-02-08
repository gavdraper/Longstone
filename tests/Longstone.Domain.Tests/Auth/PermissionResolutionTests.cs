using FluentAssertions;
using Longstone.Domain.Auth;

namespace Longstone.Domain.Tests.Auth;

public class PermissionResolutionTests
{
    [Fact]
    public void Permission_HasAllPrdDefinedValues()
    {
        var permissions = Enum.GetValues<Permission>();

        permissions.Should().Contain(Permission.ViewPortfolios);
        permissions.Should().Contain(Permission.CreateOrders);
        permissions.Should().Contain(Permission.ExecuteOrders);
        permissions.Should().Contain(Permission.ConfigureCompliance);
        permissions.Should().Contain(Permission.OverrideComplianceBreach);
        permissions.Should().Contain(Permission.ProcessCorporateActions);
        permissions.Should().Contain(Permission.RunNavCalculation);
        permissions.Should().Contain(Permission.ViewRiskDashboards);
        permissions.Should().Contain(Permission.ManageFunds);
        permissions.Should().Contain(Permission.ManageUsers);
        permissions.Should().Contain(Permission.ViewAuditLogs);
    }

    [Fact]
    public void Permission_HasExactlyElevenValues()
    {
        Enum.GetValues<Permission>().Should().HaveCount(11);
    }

    [Fact]
    public void PermissionScope_HasOwnAndAll()
    {
        var scopes = Enum.GetValues<PermissionScope>();

        scopes.Should().Contain(PermissionScope.Own);
        scopes.Should().Contain(PermissionScope.All);
        scopes.Should().HaveCount(2);
    }

    [Fact]
    public void PermissionGrant_WithRoleDefault_ReportsCorrectSource()
    {
        var grant = PermissionGrant.FromRoleDefault(Permission.ViewPortfolios, PermissionScope.All);

        grant.Permission.Should().Be(Permission.ViewPortfolios);
        grant.Scope.Should().Be(PermissionScope.All);
        grant.IsGranted.Should().BeTrue();
        grant.Source.Should().Be(PermissionGrantSource.RoleDefault);
    }

    [Fact]
    public void PermissionGrant_WithUserOverrideGrant_ReportsCorrectSource()
    {
        var grant = PermissionGrant.FromUserOverride(Permission.ViewPortfolios, PermissionScope.All, isGranted: true);

        grant.Permission.Should().Be(Permission.ViewPortfolios);
        grant.Scope.Should().Be(PermissionScope.All);
        grant.IsGranted.Should().BeTrue();
        grant.Source.Should().Be(PermissionGrantSource.UserOverride);
    }

    [Fact]
    public void PermissionGrant_WithUserOverrideDeny_ReportsCorrectSource()
    {
        var grant = PermissionGrant.FromUserOverride(Permission.ViewPortfolios, PermissionScope.All, isGranted: false);

        grant.Permission.Should().Be(Permission.ViewPortfolios);
        grant.IsGranted.Should().BeFalse();
        grant.Source.Should().Be(PermissionGrantSource.UserOverride);
    }

    [Fact]
    public void PermissionGrant_Denied_HasNoScope()
    {
        var grant = PermissionGrant.Denied(Permission.ViewPortfolios);

        grant.Permission.Should().Be(Permission.ViewPortfolios);
        grant.IsGranted.Should().BeFalse();
        grant.Scope.Should().BeNull();
        grant.Source.Should().Be(PermissionGrantSource.Default);
    }

    [Fact]
    public void ResolvePermission_WithNoOverrideAndNoRoleDefault_ReturnsDenied()
    {
        var result = PermissionGrant.Resolve(
            permission: Permission.ManageUsers,
            roleDefault: null,
            userOverride: null);

        result.IsGranted.Should().BeFalse();
        result.Source.Should().Be(PermissionGrantSource.Default);
    }

    [Fact]
    public void ResolvePermission_WithRoleDefaultAndNoOverride_ReturnsRoleDefault()
    {
        var roleDefault = PermissionGrant.FromRoleDefault(Permission.ViewPortfolios, PermissionScope.Own);

        var result = PermissionGrant.Resolve(
            permission: Permission.ViewPortfolios,
            roleDefault: roleDefault,
            userOverride: null);

        result.IsGranted.Should().BeTrue();
        result.Scope.Should().Be(PermissionScope.Own);
        result.Source.Should().Be(PermissionGrantSource.RoleDefault);
    }

    [Fact]
    public void ResolvePermission_WithGrantOverride_OverridesRoleDefault()
    {
        var roleDefault = PermissionGrant.FromRoleDefault(Permission.ViewPortfolios, PermissionScope.Own);
        var userOverride = PermissionGrant.FromUserOverride(Permission.ViewPortfolios, PermissionScope.All, isGranted: true);

        var result = PermissionGrant.Resolve(
            permission: Permission.ViewPortfolios,
            roleDefault: roleDefault,
            userOverride: userOverride);

        result.IsGranted.Should().BeTrue();
        result.Scope.Should().Be(PermissionScope.All);
        result.Source.Should().Be(PermissionGrantSource.UserOverride);
    }

    [Fact]
    public void ResolvePermission_WithDenyOverride_OverridesRoleDefault()
    {
        var roleDefault = PermissionGrant.FromRoleDefault(Permission.ViewPortfolios, PermissionScope.All);
        var userOverride = PermissionGrant.FromUserOverride(Permission.ViewPortfolios, PermissionScope.All, isGranted: false);

        var result = PermissionGrant.Resolve(
            permission: Permission.ViewPortfolios,
            roleDefault: roleDefault,
            userOverride: userOverride);

        result.IsGranted.Should().BeFalse();
        result.Source.Should().Be(PermissionGrantSource.UserOverride);
    }

    [Fact]
    public void ResolvePermission_WithGrantOverrideAndNoRoleDefault_GrantsAccess()
    {
        var userOverride = PermissionGrant.FromUserOverride(Permission.ManageUsers, PermissionScope.All, isGranted: true);

        var result = PermissionGrant.Resolve(
            permission: Permission.ManageUsers,
            roleDefault: null,
            userOverride: userOverride);

        result.IsGranted.Should().BeTrue();
        result.Scope.Should().Be(PermissionScope.All);
        result.Source.Should().Be(PermissionGrantSource.UserOverride);
    }

    [Fact]
    public void ResolvePermission_WithMismatchedRoleDefault_Throws()
    {
        var roleDefault = PermissionGrant.FromRoleDefault(Permission.ViewPortfolios, PermissionScope.All);

        var act = () => PermissionGrant.Resolve(
            permission: Permission.ManageUsers,
            roleDefault: roleDefault,
            userOverride: null);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("roleDefault");
    }

    [Fact]
    public void ResolvePermission_WithMismatchedUserOverride_Throws()
    {
        var userOverride = PermissionGrant.FromUserOverride(Permission.ViewPortfolios, PermissionScope.All, isGranted: true);

        var act = () => PermissionGrant.Resolve(
            permission: Permission.ManageUsers,
            roleDefault: null,
            userOverride: userOverride);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("userOverride");
    }
}
