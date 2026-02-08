namespace Longstone.Domain.Compliance;

public interface IMandateRuleRepository
{
    Task<MandateRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MandateRule>> GetByFundAsync(Guid fundId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MandateRule>> GetActiveByFundAsync(Guid fundId, CancellationToken cancellationToken = default);
    Task AddAsync(MandateRule rule, CancellationToken cancellationToken = default);
    void Update(MandateRule rule);
}
