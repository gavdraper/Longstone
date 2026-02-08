using FluentAssertions;
using Longstone.Domain.Audit;
using Longstone.Domain.Auth;
using Microsoft.Extensions.Time.Testing;

namespace Longstone.Domain.Tests.Audit;

public class AuditEventTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_WithRequiredFields_SetsAllProperties()
    {
        var auditEvent = AuditEvent.Create(
            _userId, Role.FundManager, "Create", "Order", "order-123", _timeProvider);

        auditEvent.Id.Should().NotBe(Guid.Empty);
        auditEvent.Timestamp.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        auditEvent.UserId.Should().Be(_userId);
        auditEvent.UserRole.Should().Be(Role.FundManager);
        auditEvent.Action.Should().Be("Create");
        auditEvent.EntityType.Should().Be("Order");
        auditEvent.EntityId.Should().Be("order-123");
        auditEvent.BeforeState.Should().BeNull();
        auditEvent.AfterState.Should().BeNull();
        auditEvent.Reason.Should().BeNull();
        auditEvent.IpAddress.Should().BeNull();
        auditEvent.SessionId.Should().BeNull();
        auditEvent.CorrelationId.Should().BeNull();
        auditEvent.TraceId.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllOptionalFields_SetsAllProperties()
    {
        var auditEvent = AuditEvent.Create(
            _userId, Role.SystemAdmin, "Update", "User", "user-456", _timeProvider,
            beforeState: "{\"active\":true}",
            afterState: "{\"active\":false}",
            reason: "Compliance violation",
            ipAddress: "192.168.1.1",
            sessionId: "session-789",
            correlationId: "corr-abc",
            traceId: "trace-def");

        auditEvent.BeforeState.Should().Be("{\"active\":true}");
        auditEvent.AfterState.Should().Be("{\"active\":false}");
        auditEvent.Reason.Should().Be("Compliance violation");
        auditEvent.IpAddress.Should().Be("192.168.1.1");
        auditEvent.SessionId.Should().Be("session-789");
        auditEvent.CorrelationId.Should().Be("corr-abc");
        auditEvent.TraceId.Should().Be("trace-def");
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var event1 = AuditEvent.Create(_userId, Role.Dealer, "Create", "Order", "1", _timeProvider);
        var event2 = AuditEvent.Create(_userId, Role.Dealer, "Create", "Order", "2", _timeProvider);

        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        var act = () => AuditEvent.Create(
            Guid.Empty, Role.FundManager, "Create", "Order", "order-123", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("userId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidAction_Throws(string? action)
    {
        var act = () => AuditEvent.Create(
            _userId, Role.FundManager, action!, "Order", "order-123", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("action");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEntityType_Throws(string? entityType)
    {
        var act = () => AuditEvent.Create(
            _userId, Role.FundManager, "Create", entityType!, "order-123", _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("entityType");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEntityId_Throws(string? entityId)
    {
        var act = () => AuditEvent.Create(
            _userId, Role.FundManager, "Create", "Order", entityId!, _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("entityId");
    }

    [Fact]
    public void Create_WithNullTimeProvider_Throws()
    {
        var act = () => AuditEvent.Create(
            _userId, Role.FundManager, "Create", "Order", "order-123", null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Create_UsesTimeProviderForTimestamp()
    {
        var specificTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(specificTime);

        var auditEvent = AuditEvent.Create(
            _userId, Role.FundManager, "Create", "Order", "order-123", timeProvider);

        auditEvent.Timestamp.Should().Be(specificTime.UtcDateTime);
    }
}
