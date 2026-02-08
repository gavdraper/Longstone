using FluentAssertions;
using Longstone.Domain.Auth;

namespace Longstone.Domain.Tests.Auth;

public class RolePermissionTests
{
    [Fact]
    public void Create_WithValidInputs_SetsAllProperties()
    {
        var rolePermission = RolePermission.Create(Role.FundManager, Permission.ViewPortfolios, PermissionScope.All);

        rolePermission.Id.Should().NotBe(Guid.Empty);
        rolePermission.Role.Should().Be(Role.FundManager);
        rolePermission.Permission.Should().Be(Permission.ViewPortfolios);
        rolePermission.Scope.Should().Be(PermissionScope.All);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var rp1 = RolePermission.Create(Role.Dealer, Permission.CreateOrders, PermissionScope.Own);
        var rp2 = RolePermission.Create(Role.Dealer, Permission.CreateOrders, PermissionScope.Own);

        rp1.Id.Should().NotBe(rp2.Id);
    }

    [Fact]
    public void Create_WithInvalidRole_Throws()
    {
        var act = () => RolePermission.Create((Role)999, Permission.ViewPortfolios, PermissionScope.All);

        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("role");
    }

    [Fact]
    public void Create_WithInvalidPermission_Throws()
    {
        var act = () => RolePermission.Create(Role.FundManager, (Permission)999, PermissionScope.All);

        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("permission");
    }

    [Fact]
    public void Create_WithInvalidScope_Throws()
    {
        var act = () => RolePermission.Create(Role.FundManager, Permission.ViewPortfolios, (PermissionScope)999);

        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("scope");
    }
}
