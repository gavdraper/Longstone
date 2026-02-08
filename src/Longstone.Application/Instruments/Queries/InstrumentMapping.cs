using Longstone.Domain.Instruments;

namespace Longstone.Application.Instruments.Queries;

internal static class InstrumentMapping
{
    public static InstrumentDto ToDto(Instrument instrument) =>
        new(
            Id: instrument.Id,
            Isin: instrument.Isin,
            Sedol: instrument.Sedol,
            Ticker: instrument.Ticker,
            Exchange: instrument.Exchange,
            Name: instrument.Name,
            Currency: instrument.Currency,
            CountryOfListing: instrument.CountryOfListing,
            Sector: instrument.Sector,
            AssetClass: instrument.AssetClass,
            MarketCapitalisation: instrument.MarketCapitalisation,
            Status: instrument.Status,
            CreatedAt: instrument.CreatedAt,
            UpdatedAt: instrument.UpdatedAt);
}
