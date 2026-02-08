namespace Longstone.Domain.Auth;

public class UserPermissionOverride
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Permission Permission { get; private set; }
    public PermissionScope Scope { get; private set; }
    public bool IsGranted { get; private set; }
    public Guid OverriddenBy { get; private set; }
    public DateTime OverriddenAt { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    public User? User { get; private set; }

    private UserPermissionOverride() { }

    public static UserPermissionOverride Create(
        Guid userId,
        Permission permission,
        PermissionScope scope,
        bool isGranted,
        Guid overriddenBy,
        string reason,
        TimeProvider timeProvider)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));
        if (overriddenBy == Guid.Empty)
            throw new ArgumentException("Overridden by user ID is required.", nameof(overriddenBy));
        if (userId == overriddenBy)
            throw new ArgumentException("A user cannot override their own permissions.", nameof(overriddenBy));
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentNullException.ThrowIfNull(timeProvider);

        return new UserPermissionOverride
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Permission = permission,
            Scope = scope,
            IsGranted = isGranted,
            OverriddenBy = overriddenBy,
            OverriddenAt = timeProvider.GetUtcNow().UtcDateTime,
            Reason = reason
        };
    }
}
