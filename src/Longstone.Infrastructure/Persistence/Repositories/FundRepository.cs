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
        FundSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Funds
            .Include(f => f.AssignedManagers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var term = criteria.SearchTerm.Trim().ToUpperInvariant();
            query = query.Where(f =>
                f.Name.ToUpper().Contains(term) ||
                f.Lei.ToUpper().Contains(term) ||
                f.Isin.ToUpper().Contains(term));
        }

        if (criteria.StatusFilter.HasValue)
        {
            query = query.Where(f => f.Status == criteria.StatusFilter.Value);
        }

        if (criteria.ManagerFilter.HasValue)
        {
            query = query.Where(f => f.AssignedManagers.Any(m => m.UserId == criteria.ManagerFilter.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(f => f.Name)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
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
