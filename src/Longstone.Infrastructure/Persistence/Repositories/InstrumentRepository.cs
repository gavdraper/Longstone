using Longstone.Domain.Instruments;
using Microsoft.EntityFrameworkCore;

namespace Longstone.Infrastructure.Persistence.Repositories;

public sealed class InstrumentRepository(LongstoneDbContext dbContext) : IInstrumentRepository
{
    public async Task<Instrument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Instruments
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Instrument> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        AssetClass? assetClassFilter,
        Exchange? exchangeFilter,
        string? countryFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Instruments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToUpperInvariant();
            query = query.Where(i =>
                i.Name.ToUpper().Contains(term) ||
                i.Ticker.ToUpper().Contains(term) ||
                i.Isin.ToUpper().Contains(term) ||
                i.Sedol.ToUpper().Contains(term));
        }

        if (assetClassFilter.HasValue)
        {
            query = query.Where(i => i.AssetClass == assetClassFilter.Value);
        }

        if (exchangeFilter.HasValue)
        {
            query = query.Where(i => i.Exchange == exchangeFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(countryFilter))
        {
            query = query.Where(i => i.CountryOfListing == countryFilter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Instrument?> GetByIsinAsync(string isin, CancellationToken cancellationToken = default)
    {
        return await dbContext.Instruments
            .FirstOrDefaultAsync(i => i.Isin == isin, cancellationToken);
    }

    public async Task AddAsync(Instrument instrument, CancellationToken cancellationToken = default)
    {
        await dbContext.Instruments.AddAsync(instrument, cancellationToken);
    }

    public void Update(Instrument instrument)
    {
        dbContext.Instruments.Update(instrument);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Instruments.AnyAsync(i => i.Id == id, cancellationToken);
    }
}
