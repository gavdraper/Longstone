using Longstone.Domain.Instruments;

namespace Longstone.Application.Instruments.Queries;

public sealed record InstrumentDto(
    Guid Id,
    string Isin,
    string Sedol,
    string Ticker,
    Exchange Exchange,
    string Name,
    string Currency,
    string CountryOfListing,
    string Sector,
    AssetClass AssetClass,
    decimal MarketCapitalisation,
    InstrumentStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
