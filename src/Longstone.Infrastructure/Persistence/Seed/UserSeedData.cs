using Longstone.Domain.Auth;
using Microsoft.AspNetCore.Identity;

namespace Longstone.Infrastructure.Persistence.Seed;

public static class UserSeedData
{
    public static IReadOnlyList<(string Username, string Email, string FullName, Role Role)> SeedUsers =>
    [
        ("admin", "admin@longstone.local", "System Administrator", Role.SystemAdmin),
        ("fundmgr", "fundmgr@longstone.local", "Sarah Mitchell", Role.FundManager),
        ("dealer", "dealer@longstone.local", "James Chen", Role.Dealer),
        ("compliance", "compliance@longstone.local", "Emma Richardson", Role.ComplianceOfficer),
        ("operations", "operations@longstone.local", "David Okafor", Role.Operations),
        ("risk", "risk@longstone.local", "Priya Sharma", Role.RiskManager),
        ("readonly", "readonly@longstone.local", "Alex Thompson", Role.ReadOnly),
    ];

    public static IReadOnlyList<User> CreateSeededUsers(TimeProvider timeProvider)
    {
        var hasher = new PasswordHasher<User>();
        var users = new List<User>();

        foreach (var (username, email, fullName, role) in SeedUsers)
        {
            var user = User.Create(username, email, fullName, role, "placeholder", timeProvider);
            var hash = hasher.HashPassword(user, "Dev123!");
            user.UpdatePassword(hash, timeProvider);
            users.Add(user);
        }

        return users;
    }
}
