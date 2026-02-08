using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Longstone.Domain.Audit;
using Longstone.Domain.Auth;
using Longstone.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Longstone.Infrastructure.Persistence.Interceptors;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Well-known GUID for system-initiated operations (seed, migration, unauthenticated).
    /// Uses SystemAdmin role to distinguish from user actions in compliance auditing.
    /// </summary>
    internal static readonly Guid SystemUserId = new("00000000-0000-0000-0000-000000000001");

    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TimeProvider _timeProvider;

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor, TimeProvider timeProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditEvents(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditEvents(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEvents(DbContextEventData eventData)
    {
        if (eventData.Context is null)
            return;

        var auditEvents = CreateAuditEvents(eventData.Context);
        if (auditEvents.Count > 0)
        {
            eventData.Context.Set<AuditEvent>().AddRange(auditEvents);
        }
    }

    private List<AuditEvent> CreateAuditEvents(DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable
                     && e.Entity is not AuditEvent
                     && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
            return [];

        var (userId, userRole) = GetUserContext();
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var traceId = Activity.Current?.TraceId.ToString();

        var auditEvents = new List<AuditEvent>(entries.Count);

        foreach (var entry in entries)
        {
            var auditEvent = CreateAuditEvent(entry, userId, userRole, ipAddress, traceId);
            auditEvents.Add(auditEvent);
        }

        return auditEvents;
    }

    private AuditEvent CreateAuditEvent(EntityEntry entry, Guid userId, Role userRole, string? ipAddress, string? traceId)
    {
        var action = entry.State switch
        {
            EntityState.Added => "Created",
            EntityState.Modified => "Modified",
            EntityState.Deleted => "Deleted",
            _ => throw new InvalidOperationException($"Unexpected entity state: {entry.State}")
        };

        var entityType = entry.Entity.GetType().Name;
        var entityId = GetPrimaryKeyValue(entry);
        var beforeState = entry.State is EntityState.Modified or EntityState.Deleted
            ? SerializeEntityState(entry, useOriginalValues: true)
            : null;
        var afterState = entry.State is EntityState.Added or EntityState.Modified
            ? SerializeEntityState(entry, useOriginalValues: false)
            : null;

        return AuditEvent.Create(
            userId: userId,
            userRole: userRole,
            action: action,
            entityType: entityType,
            entityId: entityId,
            timeProvider: _timeProvider,
            beforeState: beforeState,
            afterState: afterState,
            ipAddress: ipAddress,
            traceId: traceId);
    }

    private (Guid UserId, Role UserRole) GetUserContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
            return (SystemUserId, Role.SystemAdmin);

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

        var userId = Guid.TryParse(userIdClaim, out var id) && id != Guid.Empty ? id : SystemUserId;
        var userRole = Enum.TryParse<Role>(roleClaim, out var role) ? role : Role.SystemAdmin;

        return (userId, userRole);
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();
        if (primaryKey is null)
            return string.Empty;

        var keyValues = primaryKey.Properties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? string.Empty);

        return string.Join(",", keyValues);
    }

    private static string SerializeEntityState(EntityEntry entry, bool useOriginalValues)
    {
        var properties = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty())
                continue;

            var value = useOriginalValues ? property.OriginalValue : property.CurrentValue;
            properties[property.Metadata.Name] = value;
        }

        try
        {
            return JsonSerializer.Serialize(properties, SerializationOptions);
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(
                properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString()),
                SerializationOptions);
        }
    }
}
