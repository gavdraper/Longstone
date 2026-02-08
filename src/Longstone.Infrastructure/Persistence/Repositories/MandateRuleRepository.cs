using Longstone.Domain.Compliance;
using Microsoft.EntityFrameworkCore;

namespace Longstone.Infrastructure.Persistence.Repositories;

public sealed class MandateRuleRepository(LongstoneDbContext dbContext) : IMandateRuleRepository
{
    public async Task<MandateRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.MandateRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MandateRule>> GetByFundAsync(Guid fundId, CancellationToken cancellationToken = default)
    {
        return await dbContext.MandateRules
            .Where(r => r.FundId == fundId)
            .OrderBy(r => r.RuleType)
            .ThenBy(r => r.EffectiveFrom)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MandateRule>> GetActiveByFundAsync(Guid fundId, CancellationToken cancellationToken = default)
    {
        return await dbContext.MandateRules
            .Where(r => r.FundId == fundId && r.IsActive)
            .OrderBy(r => r.RuleType)
            .ThenBy(r => r.EffectiveFrom)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MandateRule rule, CancellationToken cancellationToken = default)
    {
        await dbContext.MandateRules.AddAsync(rule, cancellationToken);
    }

    public void Update(MandateRule rule)
    {
        dbContext.MandateRules.Update(rule);
    }
}
