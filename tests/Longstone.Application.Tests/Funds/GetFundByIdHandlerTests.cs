using FluentAssertions;
using Longstone.Application.Funds.Queries;
using Longstone.Application.Funds.Queries.GetFundById;
using Longstone.Domain.Funds;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Funds;

public class GetFundByIdHandlerTests
{
    private readonly IFundRepository _fundRepository = Substitute.For<IFundRepository>();
    private readonly GetFundByIdHandler _handler;

    public GetFundByIdHandlerTests()
    {
        _handler = new GetFundByIdHandler(_fundRepository);
    }

    [Fact]
    public async Task Handle_WithExistingFund_ReturnsFundDto()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
        var fund = Fund.Create("Test Fund", "549300ABCDEF123456XY", "GB00B1234567",
            FundType.OEIC, "GBP", "FTSE All-Share",
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), timeProvider);

        _fundRepository.GetByIdAsync(fund.Id, Arg.Any<CancellationToken>())
            .Returns(fund);

        var result = await _handler.Handle(new GetFundByIdQuery(fund.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(fund.Id);
        result.Name.Should().Be("Test Fund");
        result.BaseCurrency.Should().Be("GBP");
    }

    [Fact]
    public async Task Handle_WithNonExistentFund_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _fundRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Fund?)null);

        var result = await _handler.Handle(new GetFundByIdQuery(id), CancellationToken.None);

        result.Should().BeNull();
    }
}
