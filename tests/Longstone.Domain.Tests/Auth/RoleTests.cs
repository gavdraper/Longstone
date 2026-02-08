using FluentAssertions;
using Longstone.Domain.Auth;

namespace Longstone.Domain.Tests.Auth;

public class RoleTests
{
    [Fact]
    public void Role_HasAllPrdDefinedValues()
    {
        var roles = Enum.GetValues<Role>();

        roles.Should().Contain(Role.SystemAdmin);
        roles.Should().Contain(Role.FundManager);
        roles.Should().Contain(Role.Dealer);
        roles.Should().Contain(Role.ComplianceOfficer);
        roles.Should().Contain(Role.Operations);
        roles.Should().Contain(Role.RiskManager);
        roles.Should().Contain(Role.ReadOnly);
    }

    [Fact]
    public void Role_HasExactlySevenValues()
    {
        var roles = Enum.GetValues<Role>();

        roles.Should().HaveCount(7);
    }

    [Theory]
    [InlineData(Role.SystemAdmin, "SystemAdmin")]
    [InlineData(Role.FundManager, "FundManager")]
    [InlineData(Role.Dealer, "Dealer")]
    [InlineData(Role.ComplianceOfficer, "ComplianceOfficer")]
    [InlineData(Role.Operations, "Operations")]
    [InlineData(Role.RiskManager, "RiskManager")]
    [InlineData(Role.ReadOnly, "ReadOnly")]
    public void Role_HasCorrectName(Role role, string expectedName)
    {
        role.ToString().Should().Be(expectedName);
    }
}
