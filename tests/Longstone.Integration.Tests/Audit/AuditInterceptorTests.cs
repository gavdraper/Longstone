using System.Text.Json;
using FluentAssertions;
using Longstone.Domain.Auth;
using Longstone.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Longstone.Integration.Tests.Audit;

public class AuditInterceptorTests : IClassFixture<LongstoneWebApplicationFactory>
{
    private readonly LongstoneWebApplicationFactory _factory;

    public AuditInterceptorTests(LongstoneWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreatingAuditableEntity_ProducesAuditEvent_WithAfterState()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var initialAuditCount = await dbContext.AuditEvents.CountAsync();

        var user = User.Create("auditcreatetest", "auditcreate@test.com", "Audit Create Test", Role.ReadOnly, "hashedpw", TimeProvider.System);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var auditEvents = await dbContext.AuditEvents
            .Where(e => e.EntityId == user.Id.ToString())
            .ToListAsync();

        auditEvents.Should().ContainSingle();

        var auditEvent = auditEvents.Single();
        auditEvent.Action.Should().Be("Created");
        auditEvent.EntityType.Should().Be("User");
        auditEvent.EntityId.Should().Be(user.Id.ToString());
        auditEvent.BeforeState.Should().BeNull();
        auditEvent.AfterState.Should().NotBeNullOrWhiteSpace();

        var afterState = JsonDocument.Parse(auditEvent.AfterState!);
        afterState.RootElement.GetProperty("Username").GetString().Should().Be("auditcreatetest");
        afterState.RootElement.GetProperty("Email").GetString().Should().Be("auditcreate@test.com");

        // Cleanup
        dbContext.AuditEvents.RemoveRange(auditEvents);
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdatingAuditableEntity_ProducesAuditEvent_WithBeforeAndAfterState()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        // Create a user to modify
        var user = User.Create("auditupdatetest", "auditupdate@test.com", "Audit Update Test", Role.ReadOnly, "hashedpw", TimeProvider.System);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Clear the "Created" audit event so we only see the update
        var createdEvents = await dbContext.AuditEvents
            .Where(e => e.EntityId == user.Id.ToString())
            .ToListAsync();
        dbContext.AuditEvents.RemoveRange(createdEvents);
        await dbContext.SaveChangesAsync();

        // Modify the entity
        user.Deactivate(TimeProvider.System);
        await dbContext.SaveChangesAsync();

        var auditEvents = await dbContext.AuditEvents
            .Where(e => e.EntityId == user.Id.ToString() && e.Action == "Modified")
            .ToListAsync();

        auditEvents.Should().ContainSingle();

        var auditEvent = auditEvents.Single();
        auditEvent.Action.Should().Be("Modified");
        auditEvent.EntityType.Should().Be("User");
        auditEvent.BeforeState.Should().NotBeNullOrWhiteSpace();
        auditEvent.AfterState.Should().NotBeNullOrWhiteSpace();

        var beforeState = JsonDocument.Parse(auditEvent.BeforeState!);
        var afterState = JsonDocument.Parse(auditEvent.AfterState!);

        beforeState.RootElement.GetProperty("IsActive").GetBoolean().Should().BeTrue();
        afterState.RootElement.GetProperty("IsActive").GetBoolean().Should().BeFalse();

        // Cleanup
        dbContext.AuditEvents.RemoveRange(auditEvents);
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task DeletingAuditableEntity_ProducesAuditEvent_WithBeforeState()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        // Create a user to delete
        var user = User.Create("auditdeletetest", "auditdelete@test.com", "Audit Delete Test", Role.ReadOnly, "hashedpw", TimeProvider.System);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Clear the "Created" audit event
        var createdEvents = await dbContext.AuditEvents
            .Where(e => e.EntityId == user.Id.ToString())
            .ToListAsync();
        dbContext.AuditEvents.RemoveRange(createdEvents);
        await dbContext.SaveChangesAsync();

        var userId = user.Id;

        // Delete the entity
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        var auditEvents = await dbContext.AuditEvents
            .Where(e => e.EntityId == userId.ToString() && e.Action == "Deleted")
            .ToListAsync();

        auditEvents.Should().ContainSingle();

        var auditEvent = auditEvents.Single();
        auditEvent.Action.Should().Be("Deleted");
        auditEvent.EntityType.Should().Be("User");
        auditEvent.BeforeState.Should().NotBeNullOrWhiteSpace();
        auditEvent.AfterState.Should().BeNull();

        var beforeState = JsonDocument.Parse(auditEvent.BeforeState!);
        beforeState.RootElement.GetProperty("Username").GetString().Should().Be("auditdeletetest");

        // Cleanup
        dbContext.AuditEvents.RemoveRange(auditEvents);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task ModifyingNonAuditableEntity_DoesNotProduceAuditEvent()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var initialAuditCount = await dbContext.AuditEvents.CountAsync();

        // RolePermission does NOT implement IAuditable
        var rolePermission = RolePermission.Create(Role.ReadOnly, Permission.RunNavCalculation, PermissionScope.Own);
        dbContext.RolePermissions.Add(rolePermission);
        await dbContext.SaveChangesAsync();

        var finalAuditCount = await dbContext.AuditEvents.CountAsync();
        finalAuditCount.Should().Be(initialAuditCount, "non-IAuditable entities should not produce audit events");

        // Cleanup
        dbContext.RolePermissions.Remove(rolePermission);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task AuditEvent_CapturesTimestamp()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var beforeTimestamp = DateTime.UtcNow.AddSeconds(-1);

        var user = User.Create("audittimetest", "audittime@test.com", "Audit Time Test", Role.ReadOnly, "hashedpw", TimeProvider.System);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var afterTimestamp = DateTime.UtcNow.AddSeconds(1);

        var auditEvent = await dbContext.AuditEvents
            .FirstAsync(e => e.EntityId == user.Id.ToString());

        auditEvent.Timestamp.Should().BeAfter(beforeTimestamp);
        auditEvent.Timestamp.Should().BeBefore(afterTimestamp);

        // Cleanup
        dbContext.AuditEvents.Remove(auditEvent);
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task MultipleChangesInSingleSave_ProducesMultipleAuditEvents()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        var user1 = User.Create("auditmulti1", "auditmulti1@test.com", "Audit Multi 1", Role.ReadOnly, "hashedpw", TimeProvider.System);
        var user2 = User.Create("auditmulti2", "auditmulti2@test.com", "Audit Multi 2", Role.ReadOnly, "hashedpw", TimeProvider.System);
        dbContext.Users.AddRange(user1, user2);
        await dbContext.SaveChangesAsync();

        var auditEvents = await dbContext.AuditEvents
            .Where(e => e.EntityId == user1.Id.ToString() || e.EntityId == user2.Id.ToString())
            .ToListAsync();

        auditEvents.Should().HaveCount(2);
        auditEvents.Should().AllSatisfy(e => e.Action.Should().Be("Created"));

        // Cleanup
        dbContext.AuditEvents.RemoveRange(auditEvents);
        dbContext.Users.RemoveRange(user1, user2);
        await dbContext.SaveChangesAsync();
    }
}
