using Longstone.Infrastructure.Persistence;
using Longstone.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Longstone.Integration.Tests;

public class LongstoneWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext-related registrations to prevent double-registration of interceptors.
            // AddDbContext from AddInfrastructure registered option-builder actions that would also run,
            // causing duplicate interceptors. Removing these descriptors and re-registering cleanly avoids that.
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<LongstoneDbContext>)
                         || d.ServiceType == typeof(LongstoneDbContext)
                         || d.ServiceType.IsGenericType
                            && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)
                            && d.ServiceType.GenericTypeArguments[0] == typeof(LongstoneDbContext))
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Create a persistent in-memory SQLite connection shared across the test
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Build options directly to avoid action accumulation from multiple AddDbContext calls
            services.AddScoped(sp =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<LongstoneDbContext>();
                optionsBuilder.UseSqlite(_connection);
                optionsBuilder.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
                return optionsBuilder.Options;
            });

            services.AddScoped<LongstoneDbContext>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
