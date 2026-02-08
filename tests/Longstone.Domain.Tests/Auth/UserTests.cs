using FluentAssertions;
using Longstone.Domain.Auth;
using Microsoft.Extensions.Time.Testing;

namespace Longstone.Domain.Tests.Auth;

public class UserTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Create_WithValidInputs_SetsAllProperties()
    {
        var user = User.Create("jdoe", "jdoe@example.com", "John Doe", Role.FundManager, "hashed_password", _timeProvider);

        user.Id.Should().NotBe(Guid.Empty);
        user.Username.Should().Be("jdoe");
        user.Email.Should().Be("jdoe@example.com");
        user.FullName.Should().Be("John Doe");
        user.Role.Should().Be(Role.FundManager);
        user.IsActive.Should().BeTrue();
        user.PasswordHash.Should().Be("hashed_password");
        user.CreatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        user.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var user1 = User.Create("user1", "user1@example.com", "User One", Role.Dealer, "hash1", _timeProvider);
        var user2 = User.Create("user2", "user2@example.com", "User Two", Role.Dealer, "hash2", _timeProvider);

        user1.Id.Should().NotBe(user2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUsername_Throws(string? username)
    {
        var act = () => User.Create(username!, "email@example.com", "Full Name", Role.Dealer, "hash", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("username");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_Throws(string? email)
    {
        var act = () => User.Create("jdoe", email!, "Full Name", Role.Dealer, "hash", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFullName_Throws(string? fullName)
    {
        var act = () => User.Create("jdoe", "email@example.com", fullName!, Role.Dealer, "hash", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("fullName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPasswordHash_Throws(string? passwordHash)
    {
        var act = () => User.Create("jdoe", "email@example.com", "Full Name", Role.Dealer, passwordHash!, _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("passwordHash");
    }

    [Fact]
    public void Create_WithNullTimeProvider_Throws()
    {
        var act = () => User.Create("jdoe", "email@example.com", "Full Name", Role.Dealer, "hash", null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalseAndUpdatesTimestamp()
    {
        var user = User.Create("jdoe", "email@example.com", "John Doe", Role.FundManager, "hash", _timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        user.Deactivate(_timeProvider);

        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        user.UpdatedAt.Should().BeAfter(user.CreatedAt);
    }

    [Fact]
    public void Activate_SetsIsActiveTrueAndUpdatesTimestamp()
    {
        var user = User.Create("jdoe", "email@example.com", "John Doe", Role.FundManager, "hash", _timeProvider);
        user.Deactivate(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        user.Activate(_timeProvider);

        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void UpdatePassword_ChangesHashAndUpdatesTimestamp()
    {
        var user = User.Create("jdoe", "email@example.com", "John Doe", Role.FundManager, "old_hash", _timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        user.UpdatePassword("new_hash", _timeProvider);

        user.PasswordHash.Should().Be("new_hash");
        user.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        user.UpdatedAt.Should().BeAfter(user.CreatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdatePassword_WithInvalidHash_Throws(string? passwordHash)
    {
        var user = User.Create("jdoe", "email@example.com", "John Doe", Role.FundManager, "hash", _timeProvider);

        var act = () => user.UpdatePassword(passwordHash!, _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("passwordHash");
    }

    [Fact]
    public void Create_IsActiveByDefault()
    {
        var user = User.Create("jdoe", "email@example.com", "John Doe", Role.ReadOnly, "hash", _timeProvider);

        user.IsActive.Should().BeTrue();
    }
}
