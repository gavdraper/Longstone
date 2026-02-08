using Longstone.Infrastructure.Persistence;
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
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<LongstoneDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Create a persistent in-memory SQLite connection shared across the test
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Re-register with in-memory SQLite (same provider, no dual provider conflict)
            services.AddDbContext<LongstoneDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
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
