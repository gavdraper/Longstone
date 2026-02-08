using FluentAssertions;
using Longstone.Application.Instruments.Queries;
using Longstone.Application.Instruments.Queries.SearchInstruments;
using Longstone.Domain.Instruments;
using NSubstitute;

namespace Longstone.Application.Tests.Instruments;

public class SearchInstrumentsHandlerTests
{
    private readonly IInstrumentRepository _instrumentRepository = Substitute.For<IInstrumentRepository>();
    private readonly SearchInstrumentsHandler _handler;
    private readonly TimeProvider _timeProvider;

    public SearchInstrumentsHandlerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));

        _handler = new SearchInstrumentsHandler(_instrumentRepository);
    }

    private Instrument CreateInstrument(
        string name = "Test Equity",
        string ticker = "TST",
        string isin = "GB0000000001",
        AssetClass assetClass = AssetClass.Equity,
        Exchange exchange = Exchange.LSE)
    {
        return Instrument.Create(
            isin: isin,
            sedol: "B123456",
            ticker: ticker,
            exchange: exchange,
            name: name,
            currency: "GBP",
            countryOfListing: "GB",
            sector: "Technology",
            assetClass: assetClass,
            marketCapitalisation: 1_000_000_000m,
            timeProvider: _timeProvider);
    }

    [Fact]
    public async Task Handle_SearchByTicker_ReturnsMatch()
    {
        var instrument = CreateInstrument(name: "Shell plc", ticker: "SHEL", isin: "GB00BP6MXD84");
        _instrumentRepository.SearchAsync("SHEL", null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Instrument> { instrument }.AsReadOnly() as IReadOnlyList<Instrument>, 1));

        var query = new SearchInstrumentsQuery(SearchTerm: "SHEL");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Ticker.Should().Be("SHEL");
        result.Items[0].Name.Should().Be("Shell plc");
    }

    [Fact]
    public async Task Handle_SearchByIsin_ReturnsMatch()
    {
        var instrument = CreateInstrument(name: "AstraZeneca", ticker: "AZN", isin: "GB0009895292");
        _instrumentRepository.SearchAsync("GB0009895292", null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Instrument> { instrument }.AsReadOnly() as IReadOnlyList<Instrument>, 1));

        var query = new SearchInstrumentsQuery(SearchTerm: "GB0009895292");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Isin.Should().Be("GB0009895292");
    }

    [Fact]
    public async Task Handle_FilterByAssetClass_PassesFilterToRepository()
    {
        var etf = CreateInstrument(name: "Vanguard FTSE All-World", ticker: "VWRL", isin: "IE00B3RBWM25", assetClass: AssetClass.ETF);
        _instrumentRepository.SearchAsync(null, AssetClass.ETF, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Instrument> { etf }.AsReadOnly() as IReadOnlyList<Instrument>, 1));

        var query = new SearchInstrumentsQuery(AssetClassFilter: AssetClass.ETF);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].AssetClass.Should().Be(AssetClass.ETF);
        await _instrumentRepository.Received(1).SearchAsync(null, AssetClass.ETF, null, null, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FilterByExchange_PassesFilterToRepository()
    {
        var instrument = CreateInstrument(name: "Apple Inc", ticker: "AAPL", isin: "US0378331005", exchange: Exchange.NASDAQ);
        _instrumentRepository.SearchAsync(null, null, Exchange.NASDAQ, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Instrument> { instrument }.AsReadOnly() as IReadOnlyList<Instrument>, 1));

        var query = new SearchInstrumentsQuery(ExchangeFilter: Exchange.NASDAQ);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Exchange.Should().Be(Exchange.NASDAQ);
        await _instrumentRepository.Received(1).SearchAsync(null, null, Exchange.NASDAQ, null, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptySearch_ReturnsPaginatedAll()
    {
        var instruments = new List<Instrument>
        {
            CreateInstrument(name: "Shell plc", ticker: "SHEL", isin: "GB00BP6MXD84"),
            CreateInstrument(name: "AstraZeneca", ticker: "AZN", isin: "GB0009895292")
        };
        _instrumentRepository.SearchAsync(null, null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((instruments.AsReadOnly() as IReadOnlyList<Instrument>, 50));

        var query = new SearchInstrumentsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(50);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_WithCustomPagination_PassesPaginationToRepository()
    {
        var instruments = new List<Instrument> { CreateInstrument() };
        _instrumentRepository.SearchAsync(null, null, null, null, 3, 10, Arg.Any<CancellationToken>())
            .Returns((instruments.AsReadOnly() as IReadOnlyList<Instrument>, 25));

        var query = new SearchInstrumentsQuery(Page: 3, PageSize: 10);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyPaginatedList()
    {
        _instrumentRepository.SearchAsync(null, null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Instrument>() as IReadOnlyList<Instrument>, 0));

        var query = new SearchInstrumentsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MapsInstrumentDtoCorrectly()
    {
        var instrument = CreateInstrument(
            name: "Shell plc",
            ticker: "SHEL",
            isin: "GB00BP6MXD84",
            assetClass: AssetClass.Equity,
            exchange: Exchange.LSE);
        _instrumentRepository.SearchAsync(null, null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Instrument> { instrument }.AsReadOnly() as IReadOnlyList<Instrument>, 1));

        var query = new SearchInstrumentsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Items[0];
        dto.Id.Should().Be(instrument.Id);
        dto.Isin.Should().Be("GB00BP6MXD84");
        dto.Sedol.Should().Be("B123456");
        dto.Ticker.Should().Be("SHEL");
        dto.Exchange.Should().Be(Exchange.LSE);
        dto.Name.Should().Be("Shell plc");
        dto.Currency.Should().Be("GBP");
        dto.CountryOfListing.Should().Be("GB");
        dto.Sector.Should().Be("Technology");
        dto.AssetClass.Should().Be(AssetClass.Equity);
        dto.MarketCapitalisation.Should().Be(1_000_000_000m);
        dto.Status.Should().Be(InstrumentStatus.Active);
    }
}
