using Longstone.Domain.Auth;

namespace Longstone.Domain.Audit;

public class AuditEvent
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Guid UserId { get; private set; }
    public Role UserRole { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string? BeforeState { get; private set; }
    public string? AfterState { get; private set; }
    public string? Reason { get; private set; }
    public string? IpAddress { get; private set; }
    public string? SessionId { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? TraceId { get; private set; }

    private AuditEvent() { }

    public static AuditEvent Create(
        Guid userId,
        Role userRole,
        string action,
        string entityType,
        string entityId,
        TimeProvider timeProvider,
        string? beforeState = null,
        string? afterState = null,
        string? reason = null,
        string? ipAddress = null,
        string? sessionId = null,
        string? correlationId = null,
        string? traceId = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        ArgumentNullException.ThrowIfNull(timeProvider);

        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            Timestamp = timeProvider.GetUtcNow().UtcDateTime,
            UserId = userId,
            UserRole = userRole,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeState = beforeState,
            AfterState = afterState,
            Reason = reason,
            IpAddress = ipAddress,
            SessionId = sessionId,
            CorrelationId = correlationId,
            TraceId = traceId
        };
    }
}
