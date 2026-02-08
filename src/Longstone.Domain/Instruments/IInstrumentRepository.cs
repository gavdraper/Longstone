namespace Longstone.Domain.Instruments;

public interface IInstrumentRepository
{
    Task<Instrument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Instrument> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        AssetClass? assetClassFilter,
        Exchange? exchangeFilter,
        string? countryFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Instrument?> GetByIsinAsync(string isin, CancellationToken cancellationToken = default);
    Task AddAsync(Instrument instrument, CancellationToken cancellationToken = default);
    void Update(Instrument instrument);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
