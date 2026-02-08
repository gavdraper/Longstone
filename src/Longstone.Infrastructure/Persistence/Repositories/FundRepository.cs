using Longstone.Domain.Funds;
using Microsoft.EntityFrameworkCore;

namespace Longstone.Infrastructure.Persistence.Repositories;

public sealed class FundRepository(LongstoneDbContext dbContext) : IFundRepository
{
    public async Task<Fund?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Funds
            .Include(f => f.AssignedManagers)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Fund>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Funds
            .Include(f => f.AssignedManagers)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Fund>> GetByStatusAsync(FundStatus status, CancellationToken cancellationToken = default)
    {
        return await dbContext.Funds
            .Include(f => f.AssignedManagers)
            .Where(f => f.Status == status)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Fund>> GetByManagerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Funds
            .Include(f => f.AssignedManagers)
            .Where(f => f.AssignedManagers.Any(m => m.UserId == userId))
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Fund> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        FundStatus? statusFilter,
        Guid? managerFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Funds
            .Include(f => f.AssignedManagers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToUpperInvariant();
            query = query.Where(f =>
                f.Name.ToUpper().Contains(term) ||
                f.Lei.ToUpper().Contains(term) ||
                f.Isin.ToUpper().Contains(term));
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(f => f.Status == statusFilter.Value);
        }

        if (managerFilter.HasValue)
        {
            query = query.Where(f => f.AssignedManagers.Any(m => m.UserId == managerFilter.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Fund fund, CancellationToken cancellationToken = default)
    {
        await dbContext.Funds.AddAsync(fund, cancellationToken);
    }

    public void Update(Fund fund)
    {
        dbContext.Funds.Update(fund);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Funds.AnyAsync(f => f.Id == id, cancellationToken);
    }
}
