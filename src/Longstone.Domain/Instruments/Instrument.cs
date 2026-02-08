using Longstone.Domain.Common;

namespace Longstone.Domain.Instruments;

public class Instrument : IAuditable
{
    public Guid Id { get; private set; }
    public string Isin { get; private set; } = string.Empty;
    public string Sedol { get; private set; } = string.Empty;
    public string Ticker { get; private set; } = string.Empty;
    public Exchange Exchange { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public string CountryOfListing { get; private set; } = string.Empty;
    public string Sector { get; private set; } = string.Empty;
    public AssetClass AssetClass { get; private set; }
    public decimal MarketCapitalisation { get; private set; }
    public InstrumentStatus Status { get; private set; }
    public FixedIncomeDetails? FixedIncomeDetails { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Instrument() { }

    public static Instrument Create(
        string isin,
        string sedol,
        string ticker,
        Exchange exchange,
        string name,
        string currency,
        string countryOfListing,
        string sector,
        AssetClass assetClass,
        decimal marketCapitalisation,
        TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(isin);
        ArgumentException.ThrowIfNullOrWhiteSpace(sedol);
        ArgumentException.ThrowIfNullOrWhiteSpace(ticker);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryOfListing);
        ArgumentException.ThrowIfNullOrWhiteSpace(sector);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (marketCapitalisation < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(marketCapitalisation), "Market capitalisation cannot be negative.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        return new Instrument
        {
            Id = Guid.NewGuid(),
            Isin = isin,
            Sedol = sedol,
            Ticker = ticker,
            Exchange = exchange,
            Name = name,
            Currency = currency,
            CountryOfListing = countryOfListing,
            Sector = sector,
            AssetClass = assetClass,
            MarketCapitalisation = marketCapitalisation,
            Status = InstrumentStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Suspend(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (Status == InstrumentStatus.Delisted)
        {
            throw new InvalidOperationException("Cannot suspend a delisted instrument.");
        }

        Status = InstrumentStatus.Suspended;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Delist(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        Status = InstrumentStatus.Delisted;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Reactivate(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (Status == InstrumentStatus.Delisted)
        {
            throw new InvalidOperationException("Cannot reactivate a delisted instrument.");
        }

        Status = InstrumentStatus.Active;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void SetFixedIncomeDetails(FixedIncomeDetails details, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (AssetClass != AssetClass.FixedIncome)
        {
            throw new InvalidOperationException($"Fixed income details can only be set on {AssetClass.FixedIncome} instruments.");
        }

        FixedIncomeDetails = details;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }
}
