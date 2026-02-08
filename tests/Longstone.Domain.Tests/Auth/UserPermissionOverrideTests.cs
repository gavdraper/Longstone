using FluentAssertions;
using Longstone.Domain.Auth;
using Microsoft.Extensions.Time.Testing;

namespace Longstone.Domain.Tests.Auth;

public class UserPermissionOverrideTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _adminId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInputs_SetsAllProperties()
    {
        var overrideEntity = UserPermissionOverride.Create(
            _userId, Permission.ExecuteOrders, PermissionScope.All,
            isGranted: true, _adminId, "Business requirement", _timeProvider);

        overrideEntity.Id.Should().NotBe(Guid.Empty);
        overrideEntity.UserId.Should().Be(_userId);
        overrideEntity.Permission.Should().Be(Permission.ExecuteOrders);
        overrideEntity.Scope.Should().Be(PermissionScope.All);
        overrideEntity.IsGranted.Should().BeTrue();
        overrideEntity.OverriddenBy.Should().Be(_adminId);
        overrideEntity.OverriddenAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        overrideEntity.Reason.Should().Be("Business requirement");
    }

    [Fact]
    public void Create_WithDeny_SetsIsGrantedFalse()
    {
        var overrideEntity = UserPermissionOverride.Create(
            _userId, Permission.ExecuteOrders, PermissionScope.All,
            isGranted: false, _adminId, "Security concern", _timeProvider);

        overrideEntity.IsGranted.Should().BeFalse();
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var o1 = UserPermissionOverride.Create(
            _userId, Permission.ViewPortfolios, PermissionScope.Own,
            true, _adminId, "Reason 1", _timeProvider);
        var o2 = UserPermissionOverride.Create(
            _userId, Permission.ViewPortfolios, PermissionScope.Own,
            true, _adminId, "Reason 2", _timeProvider);

        o1.Id.Should().NotBe(o2.Id);
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        var act = () => UserPermissionOverride.Create(
            Guid.Empty, Permission.ViewPortfolios, PermissionScope.All,
            true, _adminId, "Reason", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void Create_WithEmptyOverriddenBy_Throws()
    {
        var act = () => UserPermissionOverride.Create(
            _userId, Permission.ViewPortfolios, PermissionScope.All,
            true, Guid.Empty, "Reason", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("overriddenBy");
    }

    [Fact]
    public void Create_WithSelfOverride_Throws()
    {
        var act = () => UserPermissionOverride.Create(
            _userId, Permission.ViewPortfolios, PermissionScope.All,
            true, _userId, "Self-grant attempt", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("overriddenBy");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidReason_Throws(string? reason)
    {
        var act = () => UserPermissionOverride.Create(
            _userId, Permission.ViewPortfolios, PermissionScope.All,
            true, _adminId, reason!, _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("reason");
    }

    [Fact]
    public void Create_WithNullTimeProvider_Throws()
    {
        var act = () => UserPermissionOverride.Create(
            _userId, Permission.ViewPortfolios, PermissionScope.All,
            true, _adminId, "Reason", null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Create_UsesTimeProviderForOverriddenAt()
    {
        var specificTime = new DateTimeOffset(2025, 3, 20, 14, 30, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(specificTime);

        var overrideEntity = UserPermissionOverride.Create(
            _userId, Permission.ViewPortfolios, PermissionScope.All,
            true, _adminId, "Reason", timeProvider);

        overrideEntity.OverriddenAt.Should().Be(specificTime.UtcDateTime);
    }
}
