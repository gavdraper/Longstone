using Longstone.Domain.Auth;
using Longstone.Infrastructure.Auth;
using Longstone.Infrastructure.Persistence;
using Longstone.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Longstone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Longstone")
            ?? throw new InvalidOperationException("Connection string 'Longstone' is not configured.");

        services.AddDbContext<LongstoneDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    public static async Task InitialiseDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (!environment.IsDevelopment())
            return;

        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        await dbContext.Database.MigrateAsync();
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");

        await SeedDataAsync(dbContext);
    }

    private static async Task SeedDataAsync(LongstoneDbContext dbContext)
    {
        if (await dbContext.Users.AnyAsync())
            return;

        var users = UserSeedData.CreateSeededUsers(TimeProvider.System);
        dbContext.Users.AddRange(users);

        var rolePermissions = RolePermissionSeedData.CreateSeededRolePermissions();
        dbContext.RolePermissions.AddRange(rolePermissions);

        await dbContext.SaveChangesAsync();
    }
}
