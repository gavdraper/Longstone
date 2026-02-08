using FluentAssertions;
using Longstone.Domain.Instruments;
using Microsoft.Extensions.Time.Testing;

namespace Longstone.Domain.Tests.Instruments;

public class InstrumentTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private Instrument CreateValidInstrument(
        string isin = "GB0007188757",
        string sedol = "0718875",
        string ticker = "RIO",
        Exchange exchange = Exchange.LSE,
        string name = "Rio Tinto plc",
        string currency = "GBP",
        string countryOfListing = "GB",
        string sector = "Basic Materials",
        AssetClass assetClass = AssetClass.Equity,
        decimal marketCapitalisation = 50_000_000_000m)
    {
        return Instrument.Create(isin, sedol, ticker, exchange, name, currency, countryOfListing, sector, assetClass, marketCapitalisation, _timeProvider);
    }

    [Fact]
    public void Create_WithValidInputs_SetsAllProperties()
    {
        var instrument = CreateValidInstrument();

        instrument.Id.Should().NotBe(Guid.Empty);
        instrument.Isin.Should().Be("GB0007188757");
        instrument.Sedol.Should().Be("0718875");
        instrument.Ticker.Should().Be("RIO");
        instrument.Exchange.Should().Be(Exchange.LSE);
        instrument.Name.Should().Be("Rio Tinto plc");
        instrument.Currency.Should().Be("GBP");
        instrument.CountryOfListing.Should().Be("GB");
        instrument.Sector.Should().Be("Basic Materials");
        instrument.AssetClass.Should().Be(AssetClass.Equity);
        instrument.MarketCapitalisation.Should().Be(50_000_000_000m);
        instrument.Status.Should().Be(InstrumentStatus.Active);
        instrument.CreatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        instrument.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var instrument1 = CreateValidInstrument(isin: "GB0007188757", sedol: "0718875", ticker: "RIO");
        var instrument2 = CreateValidInstrument(isin: "GB00B03MLX29", sedol: "B03MLX2", ticker: "RR");

        instrument1.Id.Should().NotBe(instrument2.Id);
    }

    [Fact]
    public void Create_DefaultsToActiveStatus()
    {
        var instrument = CreateValidInstrument();

        instrument.Status.Should().Be(InstrumentStatus.Active);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidIsin_Throws(string? isin)
    {
        var act = () => CreateValidInstrument(isin: isin!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("isin");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSedol_Throws(string? sedol)
    {
        var act = () => CreateValidInstrument(sedol: sedol!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("sedol");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTicker_Throws(string? ticker)
    {
        var act = () => CreateValidInstrument(ticker: ticker!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("ticker");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_Throws(string? name)
    {
        var act = () => CreateValidInstrument(name: name!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCurrency_Throws(string? currency)
    {
        var act = () => CreateValidInstrument(currency: currency!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("currency");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCountryOfListing_Throws(string? countryOfListing)
    {
        var act = () => CreateValidInstrument(countryOfListing: countryOfListing!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("countryOfListing");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSector_Throws(string? sector)
    {
        var act = () => CreateValidInstrument(sector: sector!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("sector");
    }

    [Fact]
    public void Create_WithNullTimeProvider_Throws()
    {
        var act = () => Instrument.Create("GB0007188757", "0718875", "RIO", Exchange.LSE, "Rio Tinto plc", "GBP", "GB", "Basic Materials", AssetClass.Equity, 50_000_000_000m, null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Create_WithNegativeMarketCap_Throws()
    {
        var act = () => CreateValidInstrument(marketCapitalisation: -1m);

        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("marketCapitalisation");
    }

    [Fact]
    public void Create_WithZeroMarketCap_Succeeds()
    {
        var instrument = CreateValidInstrument(marketCapitalisation: 0m);

        instrument.MarketCapitalisation.Should().Be(0m);
    }

    // Status transition tests

    [Fact]
    public void Suspend_FromActive_SetsSuspendedAndUpdatesTimestamp()
    {
        var instrument = CreateValidInstrument();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        instrument.Suspend(_timeProvider);

        instrument.Status.Should().Be(InstrumentStatus.Suspended);
        instrument.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        instrument.UpdatedAt.Should().BeAfter(instrument.CreatedAt);
    }

    [Fact]
    public void Suspend_FromSuspended_RemainsSuspendedAndUpdatesTimestamp()
    {
        var instrument = CreateValidInstrument();
        instrument.Suspend(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        instrument.Suspend(_timeProvider);

        instrument.Status.Should().Be(InstrumentStatus.Suspended);
        instrument.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Suspend_FromDelisted_Throws()
    {
        var instrument = CreateValidInstrument();
        instrument.Delist(_timeProvider);

        var act = () => instrument.Suspend(_timeProvider);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot suspend a delisted instrument.");
    }

    [Fact]
    public void Delist_FromActive_SetsDelistedAndUpdatesTimestamp()
    {
        var instrument = CreateValidInstrument();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        instrument.Delist(_timeProvider);

        instrument.Status.Should().Be(InstrumentStatus.Delisted);
        instrument.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        instrument.UpdatedAt.Should().BeAfter(instrument.CreatedAt);
    }

    [Fact]
    public void Delist_FromSuspended_SetsDelisted()
    {
        var instrument = CreateValidInstrument();
        instrument.Suspend(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        instrument.Delist(_timeProvider);

        instrument.Status.Should().Be(InstrumentStatus.Delisted);
    }

    [Fact]
    public void Reactivate_FromSuspended_SetsActive()
    {
        var instrument = CreateValidInstrument();
        instrument.Suspend(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        instrument.Reactivate(_timeProvider);

        instrument.Status.Should().Be(InstrumentStatus.Active);
        instrument.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Reactivate_FromActive_RemainsActiveAndUpdatesTimestamp()
    {
        var instrument = CreateValidInstrument();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        instrument.Reactivate(_timeProvider);

        instrument.Status.Should().Be(InstrumentStatus.Active);
        instrument.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Reactivate_FromDelisted_Throws()
    {
        var instrument = CreateValidInstrument();
        instrument.Delist(_timeProvider);

        var act = () => instrument.Reactivate(_timeProvider);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot reactivate a delisted instrument.");
    }

    // Asset class classification tests

    [Theory]
    [InlineData(AssetClass.Equity)]
    [InlineData(AssetClass.FixedIncome)]
    [InlineData(AssetClass.ETF)]
    [InlineData(AssetClass.Fund)]
    [InlineData(AssetClass.Cash)]
    [InlineData(AssetClass.Alternative)]
    public void Create_WithEachAssetClass_SetsCorrectly(AssetClass assetClass)
    {
        var instrument = CreateValidInstrument(assetClass: assetClass);

        instrument.AssetClass.Should().Be(assetClass);
    }

    [Fact]
    public void AssetClass_HasExactlySixValues()
    {
        var values = Enum.GetValues<AssetClass>();

        values.Should().HaveCount(6);
    }

    [Fact]
    public void InstrumentStatus_HasExactlyThreeValues()
    {
        var values = Enum.GetValues<InstrumentStatus>();

        values.Should().HaveCount(3);
    }

    [Fact]
    public void Exchange_HasExpectedValues()
    {
        var values = Enum.GetValues<Exchange>();

        values.Should().Contain(Exchange.LSE);
        values.Should().Contain(Exchange.NYSE);
        values.Should().Contain(Exchange.NASDAQ);
        values.Should().Contain(Exchange.Euronext);
        values.Should().Contain(Exchange.XETRA);
        values.Should().HaveCount(5);
    }

    // Null TimeProvider on state transitions

    [Fact]
    public void Suspend_WithNullTimeProvider_Throws()
    {
        var instrument = CreateValidInstrument();

        var act = () => instrument.Suspend(null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Delist_WithNullTimeProvider_Throws()
    {
        var instrument = CreateValidInstrument();

        var act = () => instrument.Delist(null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Reactivate_WithNullTimeProvider_Throws()
    {
        var instrument = CreateValidInstrument();
        instrument.Suspend(_timeProvider);

        var act = () => instrument.Reactivate(null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }
}
