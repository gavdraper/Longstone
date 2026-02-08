using FluentAssertions;
using Longstone.Application.Instruments.Queries;
using Longstone.Application.Instruments.Queries.GetInstrumentById;
using Longstone.Domain.Instruments;
using NSubstitute;

namespace Longstone.Application.Tests.Instruments;

public class GetInstrumentByIdHandlerTests
{
    private readonly IInstrumentRepository _instrumentRepository = Substitute.For<IInstrumentRepository>();
    private readonly GetInstrumentByIdHandler _handler;
    private readonly TimeProvider _timeProvider;

    public GetInstrumentByIdHandlerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));

        _handler = new GetInstrumentByIdHandler(_instrumentRepository);
    }

    [Fact]
    public async Task Handle_WithExistingInstrument_ReturnsInstrumentDto()
    {
        var instrument = Instrument.Create(
            isin: "GB00BP6MXD84",
            sedol: "BP6MXD8",
            ticker: "SHEL",
            exchange: Exchange.LSE,
            name: "Shell plc",
            currency: "GBP",
            countryOfListing: "GB",
            sector: "Energy",
            assetClass: AssetClass.Equity,
            marketCapitalisation: 150_000_000_000m,
            timeProvider: _timeProvider);

        _instrumentRepository.GetByIdAsync(instrument.Id, Arg.Any<CancellationToken>())
            .Returns(instrument);

        var result = await _handler.Handle(new GetInstrumentByIdQuery(instrument.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(instrument.Id);
        result.Name.Should().Be("Shell plc");
        result.Ticker.Should().Be("SHEL");
        result.Isin.Should().Be("GB00BP6MXD84");
        result.AssetClass.Should().Be(AssetClass.Equity);
    }

    [Fact]
    public async Task Handle_WithNonExistentInstrument_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _instrumentRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Instrument?)null);

        var result = await _handler.Handle(new GetInstrumentByIdQuery(id), CancellationToken.None);

        result.Should().BeNull();
    }
}
