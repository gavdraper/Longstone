namespace Longstone.Domain.Funds;

public interface IFundRepository
{
    Task<Fund?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Fund>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Fund>> GetByStatusAsync(FundStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Fund>> GetByManagerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Fund> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        FundStatus? statusFilter,
        Guid? managerFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Fund fund, CancellationToken cancellationToken = default);
    void Update(Fund fund);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
