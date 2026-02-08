using Longstone.Domain.Common;

namespace Longstone.Domain.Auth;

public class User : IAuditable
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public Role Role { get; private set; }
    public bool IsActive { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private User() { }

    public static User Create(string username, string email, string fullName, Role role, string passwordHash, TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            FullName = fullName,
            Role = role,
            IsActive = true,
            PasswordHash = passwordHash,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Deactivate(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        IsActive = false;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Activate(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        IsActive = true;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void UpdatePassword(string passwordHash, TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentNullException.ThrowIfNull(timeProvider);
        PasswordHash = passwordHash;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }
}
