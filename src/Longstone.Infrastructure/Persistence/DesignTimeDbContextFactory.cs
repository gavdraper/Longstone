using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Longstone.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LongstoneDbContext>
{
    public LongstoneDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LongstoneDbContext>();
        optionsBuilder.UseSqlite("Data Source=longstone.db");

        return new LongstoneDbContext(optionsBuilder.Options);
    }
}
