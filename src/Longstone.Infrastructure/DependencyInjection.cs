using Longstone.Domain.Auth;
using Longstone.Domain.Compliance;
using Longstone.Domain.Funds;
using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.Strategies;
using Longstone.Infrastructure.Auth;
using Longstone.Infrastructure.Instruments.Strategies;
using Longstone.Infrastructure.Persistence;
using Longstone.Infrastructure.Persistence.Interceptors;
using Longstone.Infrastructure.Persistence.Repositories;
using Longstone.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Longstone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Longstone")
            ?? throw new InvalidOperationException("Connection string 'Longstone' is not configured.");

        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<LongstoneDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddScoped<IFundRepository, FundRepository>();
        services.AddScoped<IInstrumentRepository, InstrumentRepository>();
        services.AddScoped<IMandateRuleRepository, MandateRuleRepository>();

        services.AddKeyedScoped<IInstrumentValuationStrategy, DefaultValuationStrategy>(AssetClass.Equity);
        services.AddKeyedScoped<IInstrumentValuationStrategy, DefaultValuationStrategy>(AssetClass.ETF);
        services.AddKeyedScoped<IInstrumentValuationStrategy, FixedIncomeValuationStrategy>(AssetClass.FixedIncome);
        services.AddKeyedScoped<IInstrumentValuationStrategy, DefaultValuationStrategy>(AssetClass.Fund);
        services.AddKeyedScoped<IInstrumentValuationStrategy, DefaultValuationStrategy>(AssetClass.Cash);
        services.AddKeyedScoped<IInstrumentValuationStrategy, DefaultValuationStrategy>(AssetClass.Alternative);

        services.AddKeyedScoped<IInstrumentTaxStrategy, EquityTaxStrategy>(AssetClass.Equity);
        services.AddKeyedScoped<IInstrumentTaxStrategy, EtfTaxStrategy>(AssetClass.ETF);
        services.AddKeyedScoped<IInstrumentTaxStrategy, NotSupportedTaxStrategy>(AssetClass.FixedIncome);
        services.AddKeyedScoped<IInstrumentTaxStrategy, NotSupportedTaxStrategy>(AssetClass.Fund);
        services.AddKeyedScoped<IInstrumentTaxStrategy, NotSupportedTaxStrategy>(AssetClass.Cash);
        services.AddKeyedScoped<IInstrumentTaxStrategy, NotSupportedTaxStrategy>(AssetClass.Alternative);

        services.AddKeyedScoped<IInstrumentComplianceStrategy, DefaultComplianceStrategy>(AssetClass.Equity);
        services.AddKeyedScoped<IInstrumentComplianceStrategy, DefaultComplianceStrategy>(AssetClass.ETF);
        services.AddKeyedScoped<IInstrumentComplianceStrategy, DefaultComplianceStrategy>(AssetClass.FixedIncome);
        services.AddKeyedScoped<IInstrumentComplianceStrategy, DefaultComplianceStrategy>(AssetClass.Fund);
        services.AddKeyedScoped<IInstrumentComplianceStrategy, DefaultComplianceStrategy>(AssetClass.Cash);
        services.AddKeyedScoped<IInstrumentComplianceStrategy, DefaultComplianceStrategy>(AssetClass.Alternative);

        return services;
    }

    public static async Task InitialiseDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();

        await dbContext.Database.MigrateAsync();
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");

        await SeedDataAsync(dbContext);
    }

    private static async Task SeedDataAsync(LongstoneDbContext dbContext)
    {
        if (!await dbContext.Users.AnyAsync())
        {
            var users = UserSeedData.CreateSeededUsers(TimeProvider.System);
            dbContext.Users.AddRange(users);

            var rolePermissions = RolePermissionSeedData.CreateSeededRolePermissions();
            dbContext.RolePermissions.AddRange(rolePermissions);
        }

        if (!await dbContext.Instruments.AnyAsync())
        {
            var instruments = InstrumentSeedData.CreateSeededInstruments(TimeProvider.System);
            dbContext.Instruments.AddRange(instruments);
        }

        await dbContext.SaveChangesAsync();
    }
}
